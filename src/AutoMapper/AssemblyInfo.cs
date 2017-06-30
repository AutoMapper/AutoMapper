using System;
#if NET45 || NET40
using System.Security;
#endif

[assembly:CLSCompliant(true)]
#if NET45 || NET40
[assembly: AllowPartiallyTrustedCallers]
#endif
