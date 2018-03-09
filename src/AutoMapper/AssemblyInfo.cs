using System;
using System.Resources;
#if NET45 || NET40
using System.Runtime.InteropServices;
using System.Security;
#endif

[assembly: CLSCompliant(true)]
#if NET45 || NET40
[assembly: AllowPartiallyTrustedCallers]
[assembly: ComVisible(false)]
#endif

[assembly: NeutralResourcesLanguage("en")]