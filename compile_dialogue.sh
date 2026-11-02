#!/bin/env bash
set -e
mkdir -p Temp/DialogueImpl
cat >Temp/DialogueImpl/DialogueImpl.csproj <<EOF
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>10.0</LangVersion>

    <DebugSymbols>true</DebugSymbols>
    <DebugType>portable</DebugType>
    <EmbedUntrackedSources>true</EmbedUntrackedSources>
  </PropertyGroup>

  <ItemGroup>
    <Reference Include="DialogueMachine">
      <HintPath>../../Assets/Plugins/DialogueMachine.dll</HintPath>
    </Reference>
  </ItemGroup>

</Project>
EOF
ucpem run @/DialogueMachine+cli Assets/Dialogue/dialogue.dlm Temp/DialogueImpl/dialogue.cs
(cd Temp/DialogueImpl/ && dotnet build -c Debug)
cp Temp/DialogueImpl/bin/Debug/netstandard2.1/DialogueImpl.{dll,pdb} Assets/Plugins
