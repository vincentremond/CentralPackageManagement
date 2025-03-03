module CentralPackageManagement.Tests

open FluentAssertions
open NUnit.Framework

[<Test>]
[<TestCase("1.1")>]
[<TestCase("1.1.1")>]
[<TestCase("1.1.1.1")>]
[<TestCase("1.1.0-CI-20250124-144601")>]
let Test1 input =
    let parsed = Version.parse input
    let formatted = parsed.toString ()
    formatted.Should().Be(input) |> ignore
