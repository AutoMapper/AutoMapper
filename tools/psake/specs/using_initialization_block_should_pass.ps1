properties {
	$container = @{}
	$container.foo = "foo"
	$container.bar = $null
	$foo = 1
	$bar = 1
}

task default -depends TestInit

task TestInit {
  # values are:
  # 1: original
  # 2: overide
  # 3: new
  
  Assert ($container.foo -eq "foo") "$container.foo should be foo"
  Assert ($container.bar -eq "bar") "$container.bar should be bar"
  Assert ($container.baz -eq "baz") "$container.baz should be baz"
  Assert ($foo -eq 1) "$foo should be 1"
  Assert ($bar -eq 2) "$bar should be 2"
  Assert ($baz -eq 3) "$baz should be 3"
}