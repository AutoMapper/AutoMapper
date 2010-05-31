task default -depends DisplayNotice
task DisplayNotice {
  exec { msbuild /version }
}