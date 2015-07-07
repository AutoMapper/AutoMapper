properties {
  $x = $null
  $y = $null
  $z = $null
}

task default -depends TestProperties

task TestProperties { 
  Assert ($x -ne $null) "x should not be null. Run with -properties @{'x' = '1'; 'y' = '2'}"
  Assert ($y -ne $null) "y should not be null. Run with -properties @{'x' = '1'; 'y' = '2'}"
  Assert ($z -eq $null) "z should be null"
}