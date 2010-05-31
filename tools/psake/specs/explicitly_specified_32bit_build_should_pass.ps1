$framework = '3.5x86'

task default -depends MsBuild

task MsBuild {
  exec { msbuild /version }
}
