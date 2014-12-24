Framework '4.0'

task default -depends MsBuild

task MsBuild {
  exec { msbuild /version }
}
