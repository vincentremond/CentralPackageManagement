namespace CentralPackageManagement

open System
open System.IO
open System.Text
open System.Xml
open System.Xml.Linq
open CentralPackageManagement.Types

[<RequireQualifiedAccess>]
module Migrate =
    
    let toCentralPackageManagement () =
        
        let centralPackageManagementFileName = "Directory.Packages.props"

        let saveXmlWithoutDeclaration (document: XDocument) (fileName: string) =
            let utf8Encoding = UTF8Encoding(encoderShouldEmitUTF8Identifier = true)
            let xmlSettings = XmlWriterSettings()
            xmlSettings.OmitXmlDeclaration <- true
            xmlSettings.Indent <- true
            xmlSettings.Encoding <- utf8Encoding

            use streamWriter = new StreamWriter(fileName, false, utf8Encoding)
            use xmlWriter = XmlWriter.Create(streamWriter, xmlSettings)

            document.Save(xmlWriter)

        let currentCentralPackageReferences =
            if File.Exists(centralPackageManagementFileName) then
                try
                    use fileStream = new StreamReader(centralPackageManagementFileName)
                    let document = XDocument.Load(fileStream)

                    document.Root.Elements("ItemGroup")
                    |> Seq.exactlyOne
                    |> (fun itemGroup ->
                        let items = itemGroup.Elements()

                        items
                        |> Seq.map (fun child ->
                            match child.Name.LocalName with
                            | "PackageVersion" ->
                                let packageId = child.Attribute(XName.Get("Include")).Value
                                let version = child.Attribute(XName.Get("Version")).Value |> Version.parse

                                {
                                    PackageId = PackageId packageId
                                    Version = version
                                }
                            | _ -> failwith $"Unexpected element %A{child}"
                        )
                    )
                    |> Seq.toList
                with :? XmlException -> []
            else
                []

        let projects =
            Directory.GetFiles(".", "*.?sproj", SearchOption.AllDirectories) |> Seq.toList

        if Environment.GetCommandLineArgs() |> Array.contains "--clean" then

            projects
            |> List.iter (fun project ->
                let projectDocument = XDocument.Load(project)

                projectDocument.Root.Elements("ItemGroup").Elements("PackageReference")
                |> List.ofSeq
                |> List.iter (fun packageReference ->
                    let packageId = packageReference.Attribute(XName.Get("Include")).Value

                    if
                        not
                        <| (currentCentralPackageReferences
                            |> Seq.exists (fun reference -> reference.PackageId.Value = packageId))
                    then
                        packageReference.Remove()
                )

                saveXmlWithoutDeclaration projectDocument project
            )

        let newCentralPackageReferences =
            projects
            |> Seq.collect (fun project ->
                let projectDocument = XDocument.Load(project)

                let packageReferences =
                    projectDocument.Root.Elements("ItemGroup").Elements("PackageReference")
                    |> List.ofSeq
                    |> List.choose (fun packageReference ->
                        let packageId = packageReference.Attribute(XName.Get("Include")).Value

                        let versionAttribute =
                            packageReference.Attribute(XName.Get("Version")) |> Option.ofObj

                        match versionAttribute with
                        | None -> None
                        | Some versionAttribute ->
                            let version = versionAttribute.Value |> Version.parse

                            versionAttribute.Remove()

                            Some {
                                PackageId = PackageId packageId
                                Version = version
                            }
                    )

                saveXmlWithoutDeclaration projectDocument project

                packageReferences
            )
            |> Seq.toList

        let allPackageReferences =
            (currentCentralPackageReferences @ newCentralPackageReferences)
            |> Seq.groupBy (_.PackageId.Value.ToLowerInvariant())
            |> Seq.sortBy fst
            |> Seq.map (fun (_, references) -> references |> Seq.maxBy (fun reference -> reference.Version))
            |> Seq.toList

        let newCentralPackageReferencesContent =
            XDocument(
                declaration = XDeclaration("1.0", "utf-8", "yes"),
                content = [|
                    XElement(
                        "Project",
                        XElement("PropertyGroup", XElement("ManagePackageVersionsCentrally", true)),
                        XElement(
                            "ItemGroup",
                            allPackageReferences
                            |> Seq.map (fun reference ->
                                XElement(
                                    "PackageVersion",
                                    XAttribute(XName.Get("Include"), reference.PackageId.Value),
                                    XAttribute(XName.Get("Version"), reference.Version.toString ())
                                )
                            )
                        )
                    )
                |]
            )

        saveXmlWithoutDeclaration newCentralPackageReferencesContent centralPackageManagementFileName

