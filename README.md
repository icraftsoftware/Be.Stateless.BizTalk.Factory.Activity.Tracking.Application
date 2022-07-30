# Be.Stateless.BizTalk.Factory.Activity.Tracking.Application

##### Build Pipelines

[![][pipeline.mr.badge]][pipeline.mr]

[![][pipeline.ci.badge]][pipeline.ci]

##### Latest Release

[![][package.badge]][package]

[![][nuget.badge]][nuget]

[![][package.claim.store.agent.badge]][package.claim.store.agent]

[![][nuget.claim.check.schemas.badge]][nuget.claim.check.schemas]

[![][nuget.claim.check.maps.badge]][nuget.claim.check.maps]

[![][release.badge]][release]

##### Release Preview

<!-- TODO preview deployment packages -->

[![][nuget.preview.badge]][nuget.preview]

[![][nuget.claim.check.schemas.preview.badge]][nuget.claim.check.schemas.preview]

[![][nuget.claim.check.maps.preview.badge]][nuget.claim.check.maps.preview]

##### Documentation

[![][doc.main.badge]][doc.main]

[![][doc.this.badge]][doc.this]

[![][help.badge]][help]

[![][help.claim.check.schemas.badge]][help.claim.check.schemas]

[![][help.claim.check.maps.badge]][help.claim.check.maps]

[![][help.claim.store.agent.badge]][help.claim.store.agent]

## Overview

`BizTalk.Factory`'s activity model and tracking API combined with a claim check application add-on for general purpose Microsoft BizTalk Server® development.

<!-- badges -->

[doc.main.badge]: https://img.shields.io/static/v1?label=BizTalk.Factory%20SDK&message=User's%20Guide&color=8CA1AF&logo=readthedocs
[doc.main]: https://www.stateless.be/ "BizTalk.Factory SDK User's Guide"
[doc.this.badge]: https://img.shields.io/static/v1?label=Be.Stateless.BizTalk.Factory.Activity.Tracking.Application&message=User's%20Guide&color=8CA1AF&logo=readthedocs
[doc.this]: https://www.stateless.be/BizTalk/Factory/Activity/Tracking/Application "Be.Stateless.BizTalk.Factory.Activity.Tracking.Application User's Guide"
[github.badge]: https://img.shields.io/static/v1?label=Repository&message=Be.Stateless.BizTalk.Factory.Activity.Tracking.Application&logo=github
[github]: https://github.com/icraftsoftware/Be.Stateless.BizTalk.Factory.Activity.Tracking.Application "Be.Stateless.BizTalk.Factory.Activity.Tracking.Application GitHub Repository"
[help.badge]: https://img.shields.io/static/v1?label=Be.Stateless.BizTalk.Activity.Tracking&message=Developer%20Help&color=8CA1AF&logo=microsoftacademic
[help]: https://github.com/icraftsoftware/biztalk.factory.github.io/blob/master/Help/BizTalk/Activity/Tracking/README.md "Be.Stateless.BizTalk.Activity.Tracking Developer Help"
[help.claim.check.maps.badge]: https://img.shields.io/static/v1?label=Be.Stateless.BizTalk.Claim.Check.Maps&message=Developer%20Help&color=8CA1AF&logo=microsoftacademic
[help.claim.check.maps]: https://github.com/icraftsoftware/biztalk.factory.github.io/blob/master/Help/BizTalk/Claim/Check/Maps/README.md "Be.Stateless.BizTalk.Claim.Check.Maps Developer Help"
[help.claim.check.schemas.badge]: https://img.shields.io/static/v1?label=Be.Stateless.BizTalk.Claim.Check.Schemas&message=Developer%20Help&color=8CA1AF&logo=microsoftacademic
[help.claim.check.schemas]: https://github.com/icraftsoftware/biztalk.factory.github.io/blob/master/Help/BizTalk/Claim/Check/Schemas/README.md "Be.Stateless.BizTalk.Claim.Check.Schemas Developer Help"
[help.claim.store.agent.badge]: https://img.shields.io/static/v1?label=Be.Stateless.BizTalk.Claim.Store.Agent&message=Developer%20Help&color=8CA1AF&logo=microsoftacademic
[help.claim.store.agent]: https://github.com/icraftsoftware/biztalk.factory.github.io/blob/master/Help/BizTalk/Claim/Store/Agent/README.md "Be.Stateless.BizTalk.Claim.Store.Agent Developer Help"
[nuget.badge]: https://img.shields.io/nuget/v/Be.Stateless.BizTalk.Activity.Tracking.svg?label=Be.Stateless.BizTalk.Activity.Tracking&style=flat&logo=nuget
[nuget]: https://www.nuget.org/packages/Be.Stateless.BizTalk.Activity.Tracking "Be.Stateless.BizTalk.Activity.Tracking NuGet Package"
[nuget.preview.badge]: https://badge-factory.azurewebsites.net/package/icraftsoftware/be.stateless/BizTalk.Factory.Preview/Be.Stateless.BizTalk.Activity.Tracking?logo=nuget
[nuget.preview]: https://dev.azure.com/icraftsoftware/be.stateless/_packaging?_a=package&feed=BizTalk.Factory.Preview&package=Be.Stateless.BizTalk.Activity.Tracking&protocolType=NuGet "Be.Stateless.BizTalk.Activity.Tracking Preview NuGet Package"
[nuget.claim.check.maps.badge]: https://img.shields.io/nuget/v/Be.Stateless.BizTalk.Claim.Check.Maps.svg?label=Be.Stateless.BizTalk.Claim.Check.Maps&style=flat&logo=nuget
[nuget.claim.check.maps]: https://www.nuget.org/packages/Be.Stateless.BizTalk.Claim.Check.Maps "Be.Stateless.BizTalk.Claim.Check.Maps NuGet Package"
[nuget.claim.check.maps.preview.badge]: https://badge-factory.azurewebsites.net/package/icraftsoftware/be.stateless/BizTalk.Factory.Preview/Be.Stateless.BizTalk.Claim.Check.Maps?logo=nuget
[nuget.claim.check.maps.preview]: https://dev.azure.com/icraftsoftware/be.stateless/_packaging?_a=package&feed=BizTalk.Factory.Preview&package=Be.Stateless.BizTalk.Claim.Check.Maps&protocolType=NuGet "Be.Stateless.BizTalk.Claim.Check.Maps Preview NuGet Package"
[nuget.claim.check.schemas.badge]: https://img.shields.io/nuget/v/Be.Stateless.BizTalk.Claim.Check.Schemas.svg?label=Be.Stateless.BizTalk.Claim.Check.Schemas&style=flat&logo=nuget
[nuget.claim.check.schemas]: https://www.nuget.org/packages/Be.Stateless.BizTalk.Claim.Check.Schemas "Be.Stateless.BizTalk.Claim.Check.Schemas NuGet Package"
[nuget.claim.check.schemas.preview.badge]: https://badge-factory.azurewebsites.net/package/icraftsoftware/be.stateless/BizTalk.Factory.Preview/Be.Stateless.BizTalk.Claim.Check.Schemas?logo=nuget
[nuget.claim.check.schemas.preview]: https://dev.azure.com/icraftsoftware/be.stateless/_packaging?_a=package&feed=BizTalk.Factory.Preview&package=Be.Stateless.BizTalk.Claim.Check.Schemas&protocolType=NuGet "Be.Stateless.BizTalk.Claim.Check.Schemas Preview NuGet Package"
[package.badge]: https://img.shields.io/github/v/release/icraftsoftware/Be.Stateless.BizTalk.Factory.Activity.Tracking.Application?label=Be.Stateless.BizTalk.Factory.Activity.Tracking.Application.Deployment.zip&style=flat&logo=github
[package]: https://github.com/icraftsoftware/Be.Stateless.BizTalk.Factory.Activity.Tracking.Application/releases/latest/download/Be.Stateless.BizTalk.Factory.Activity.Tracking.Application.Deployment.zip
[package.claim.store.agent.badge]: https://img.shields.io/github/v/release/icraftsoftware/Be.Stateless.BizTalk.Factory.Activity.Tracking.Application?label=Be.Stateless.BizTalk.Claim.Store.Agent.Deployment.zip&style=flat&logo=github
[package.claim.store.agent]: https://github.com/icraftsoftware/Be.Stateless.BizTalk.Factory.Activity.Tracking.Application/releases/latest/download/Be.Stateless.BizTalk.Claim.Store.Agent.Deployment.zip
[pipeline.ci.badge]: https://dev.azure.com/icraftsoftware/be.stateless/_apis/build/status/Be.Stateless.BizTalk.Factory.Activity.Tracking.Application%20Continuous%20Integration?branchName=master&label=Continuous%20Integration%20Build
[pipeline.ci]: https://dev.azure.com/icraftsoftware/be.stateless/_build/latest?definitionId=75&branchName=master "Be.Stateless.BizTalk.Factory.Activity.Tracking.Application Continuous Integration Build Pipeline"
[pipeline.mr.badge]: https://dev.azure.com/icraftsoftware/be.stateless/_apis/build/status/Be.Stateless.BizTalk.Factory.Activity.Tracking.Application%20Manual%20Release?branchName=master&label=Manual%20Release%20Build
[pipeline.mr]: https://dev.azure.com/icraftsoftware/be.stateless/_build/latest?definitionId=76&branchName=master "Be.Stateless.BizTalk.Factory.Activity.Tracking.Application Manual Release Build Pipeline"
[release.badge]: https://img.shields.io/github/v/release/icraftsoftware/Be.Stateless.BizTalk.Factory.Activity.Tracking.Application?label=Release&logo=github
[release]: https://github.com/icraftsoftware/Be.Stateless.BizTalk.Factory.Activity.Tracking.Application/releases/latest "Be.Stateless.BizTalk.Factory.Activity.Tracking.Application Release"
