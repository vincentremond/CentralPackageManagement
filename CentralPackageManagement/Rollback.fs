namespace CentralPackageManagement

open System.IO
open System.Xml.Linq
open CentralPackageManagement.Types
open Pinicola.FSharp.SpectreConsole
open Spectre.Console

[<RequireQualifiedAccess>]
module Rollback =

    let fromCentralPackageManagement () =
        let centralPackageManagementFileName = "Directory.Packages.props"

        let centralPackageManagementDocument =
            XDocument.Load(centralPackageManagementFileName)

        let references =
            centralPackageManagementDocument.Root
                .Elements("ItemGroup")
                .Elements("PackageVersion")
            |> Seq.map (fun packageVersion ->
                let packageId = packageVersion.Attribute(XName.Get("Include")).Value
                let version = packageVersion.Attribute(XName.Get("Version")).Value

                {
                    PackageId = PackageId packageId
                    Version = Version.parse version
                }
            )
            |> Seq.toList

        let projectFiles =
            Directory.GetFiles(".", "*.?sproj", SearchOption.AllDirectories) |> Seq.toList

        for projectFile in projectFiles do

            AnsiConsole.markupInterpolated $"Rolling back [bold]{projectFile}[/]"

            let projectDocument = XDocument.Load(projectFile)

            let packageReferences =
                projectDocument.Root.Elements("ItemGroup").Elements("PackageReference")
                |> Seq.toList

            for packageReference in packageReferences do
                let packageId = packageReference.Attribute(XName.Get("Include")).Value

                let reference =
                    references
                    |> Seq.tryFind (fun reference -> reference.PackageId.Value = packageId)

                match reference with
                | Some reference -> packageReference.SetAttributeValue(XName.Get("Version"), reference.Version.toString ())
                | None -> packageReference.SetAttributeValue(XName.Get("Version"), "???")

            XDocument.saveXmlWithoutDeclaration projectDocument projectFile

            AnsiConsole.markupLineInterpolated $" [grey]done[/]"

        AnsiConsole.markupInterpolated $"Rolling back [bold]{centralPackageManagementFileName}[/]"
        File.Delete centralPackageManagementFileName
        AnsiConsole.markupLineInterpolated $" [grey]done[/]"
