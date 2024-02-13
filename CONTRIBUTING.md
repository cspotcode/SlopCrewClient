This is not a good guide, only notes to myself.

---

To test if code executes in the absence of SlopCrew.API dll,
I've been doing:

```shell
cd TestExe
dotnet build && rm .\bin\Debug\net8.0\slopcrew.api.dll && .\bin\Debug\net8.0\TestExe.exe
```