Task default -Depends WhatIfCheck

Task WhatIfCheck {
	Assert ($p1 -eq 'whatifcheck') '$p1 was not whatifcheck' 
}
