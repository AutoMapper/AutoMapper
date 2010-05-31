task default -depends Test

task Test -depends Compile, Clean -PreAction {"Pre-Test"} -Action { 
  "Test"
} -PostAction {"Post-Test"}

task Compile -depends Clean { 
  "Compile"
}

task Clean { 
  "Clean"
}