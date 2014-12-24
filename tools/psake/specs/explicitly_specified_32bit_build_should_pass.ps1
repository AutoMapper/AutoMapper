Framework '4.0x86'

task default -depends MsBuild

task MsBuild {
  exec { msbuild /version }
}
