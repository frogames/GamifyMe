; ModuleID = 'marshal_methods.armeabi-v7a.ll'
source_filename = "marshal_methods.armeabi-v7a.ll"
target datalayout = "e-m:e-p:32:32-Fi8-i64:64-v128:64:128-a:0:32-n32-S64"
target triple = "armv7-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [140 x ptr] zeroinitializer, align 4

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [280 x i32] [
	i32 42639949, ; 0: System.Threading.Thread => 0x28aa24d => 131
	i32 67008169, ; 1: zh-Hant\Microsoft.Maui.Controls.resources => 0x3fe76a9 => 33
	i32 72070932, ; 2: Microsoft.Maui.Graphics.dll => 0x44bb714 => 63
	i32 117431740, ; 3: System.Runtime.InteropServices => 0x6ffddbc => 122
	i32 159306688, ; 4: System.ComponentModel.Annotations => 0x97ed3c0 => 101
	i32 165246403, ; 5: Xamarin.AndroidX.Collection.dll => 0x9d975c3 => 69
	i32 182336117, ; 6: Xamarin.AndroidX.SwipeRefreshLayout.dll => 0xade3a75 => 87
	i32 195452805, ; 7: vi/Microsoft.Maui.Controls.resources.dll => 0xba65f85 => 30
	i32 199333315, ; 8: zh-HK/Microsoft.Maui.Controls.resources.dll => 0xbe195c3 => 31
	i32 205061960, ; 9: System.ComponentModel => 0xc38ff48 => 104
	i32 244348769, ; 10: Microsoft.AspNetCore.Components.Authorization => 0xe907761 => 37
	i32 251258632, ; 11: GamifyMe.App => 0xef9e708 => 95
	i32 254259026, ; 12: Microsoft.AspNetCore.Components.dll => 0xf27af52 => 36
	i32 280992041, ; 13: cs/Microsoft.Maui.Controls.resources.dll => 0x10bf9929 => 2
	i32 317674968, ; 14: vi\Microsoft.Maui.Controls.resources => 0x12ef55d8 => 30
	i32 318968648, ; 15: Xamarin.AndroidX.Activity.dll => 0x13031348 => 65
	i32 336156722, ; 16: ja/Microsoft.Maui.Controls.resources.dll => 0x14095832 => 15
	i32 342366114, ; 17: Xamarin.AndroidX.Lifecycle.Common => 0x146817a2 => 76
	i32 356389973, ; 18: it/Microsoft.Maui.Controls.resources.dll => 0x153e1455 => 14
	i32 379916513, ; 19: System.Threading.Thread.dll => 0x16a510e1 => 131
	i32 385762202, ; 20: System.Memory.dll => 0x16fe439a => 112
	i32 395744057, ; 21: _Microsoft.Android.Resource.Designer => 0x17969339 => 34
	i32 435591531, ; 22: sv/Microsoft.Maui.Controls.resources.dll => 0x19f6996b => 26
	i32 442565967, ; 23: System.Collections => 0x1a61054f => 100
	i32 450948140, ; 24: Xamarin.AndroidX.Fragment.dll => 0x1ae0ec2c => 75
	i32 459347974, ; 25: System.Runtime.Serialization.Primitives.dll => 0x1b611806 => 124
	i32 469710990, ; 26: System.dll => 0x1bff388e => 135
	i32 498788369, ; 27: System.ObjectModel => 0x1dbae811 => 118
	i32 500358224, ; 28: id/Microsoft.Maui.Controls.resources.dll => 0x1dd2dc50 => 13
	i32 503918385, ; 29: fi/Microsoft.Maui.Controls.resources.dll => 0x1e092f31 => 7
	i32 513247710, ; 30: Microsoft.Extensions.Primitives.dll => 0x1e9789de => 57
	i32 539058512, ; 31: Microsoft.Extensions.Logging => 0x20216150 => 54
	i32 571435654, ; 32: Microsoft.Extensions.FileProviders.Embedded.dll => 0x220f6a86 => 49
	i32 592146354, ; 33: pt-BR/Microsoft.Maui.Controls.resources.dll => 0x234b6fb2 => 21
	i32 598483040, ; 34: GamifyMe.Shared.dll => 0x23ac2060 => 93
	i32 627609679, ; 35: Xamarin.AndroidX.CustomView => 0x2568904f => 73
	i32 627931235, ; 36: nl\Microsoft.Maui.Controls.resources => 0x256d7863 => 19
	i32 662205335, ; 37: System.Text.Encodings.Web.dll => 0x27787397 => 128
	i32 672442732, ; 38: System.Collections.Concurrent => 0x2814a96c => 96
	i32 688181140, ; 39: ca/Microsoft.Maui.Controls.resources.dll => 0x2904cf94 => 1
	i32 690569205, ; 40: System.Xml.Linq.dll => 0x29293ff5 => 133
	i32 706645707, ; 41: ko/Microsoft.Maui.Controls.resources.dll => 0x2a1e8ecb => 16
	i32 709557578, ; 42: de/Microsoft.Maui.Controls.resources.dll => 0x2a4afd4a => 4
	i32 722857257, ; 43: System.Runtime.Loader.dll => 0x2b15ed29 => 123
	i32 759454413, ; 44: System.Net.Requests => 0x2d445acd => 116
	i32 775507847, ; 45: System.IO.Compression => 0x2e394f87 => 108
	i32 777317022, ; 46: sk\Microsoft.Maui.Controls.resources => 0x2e54ea9e => 25
	i32 789151979, ; 47: Microsoft.Extensions.Options => 0x2f0980eb => 56
	i32 804008546, ; 48: Microsoft.AspNetCore.Components.WebView.Maui => 0x2fec3262 => 41
	i32 823281589, ; 49: System.Private.Uri.dll => 0x311247b5 => 119
	i32 830298997, ; 50: System.IO.Compression.Brotli => 0x317d5b75 => 107
	i32 878954865, ; 51: System.Net.Http.Json => 0x3463c971 => 113
	i32 904024072, ; 52: System.ComponentModel.Primitives.dll => 0x35e25008 => 102
	i32 926902833, ; 53: tr/Microsoft.Maui.Controls.resources.dll => 0x373f6a31 => 28
	i32 967690846, ; 54: Xamarin.AndroidX.Lifecycle.Common.dll => 0x39adca5e => 76
	i32 992768348, ; 55: System.Collections.dll => 0x3b2c715c => 100
	i32 999186168, ; 56: Microsoft.Extensions.FileSystemGlobbing.dll => 0x3b8e5ef8 => 51
	i32 1012816738, ; 57: Xamarin.AndroidX.SavedState.dll => 0x3c5e5b62 => 86
	i32 1028951442, ; 58: Microsoft.Extensions.DependencyInjection.Abstractions => 0x3d548d92 => 46
	i32 1029334545, ; 59: da/Microsoft.Maui.Controls.resources.dll => 0x3d5a6611 => 3
	i32 1035644815, ; 60: Xamarin.AndroidX.AppCompat => 0x3dbaaf8f => 66
	i32 1044663988, ; 61: System.Linq.Expressions.dll => 0x3e444eb4 => 110
	i32 1052210849, ; 62: Xamarin.AndroidX.Lifecycle.ViewModel.dll => 0x3eb776a1 => 78
	i32 1082857460, ; 63: System.ComponentModel.TypeConverter => 0x408b17f4 => 103
	i32 1084122840, ; 64: Xamarin.Kotlin.StdLib => 0x409e66d8 => 91
	i32 1098259244, ; 65: System => 0x41761b2c => 135
	i32 1118262833, ; 66: ko\Microsoft.Maui.Controls.resources => 0x42a75631 => 16
	i32 1158641757, ; 67: GamifyMe.UI.Shared.dll => 0x450f785d => 94
	i32 1168523401, ; 68: pt\Microsoft.Maui.Controls.resources => 0x45a64089 => 22
	i32 1173126369, ; 69: Microsoft.Extensions.FileProviders.Abstractions.dll => 0x45ec7ce1 => 47
	i32 1178241025, ; 70: Xamarin.AndroidX.Navigation.Runtime.dll => 0x463a8801 => 83
	i32 1203215381, ; 71: pl/Microsoft.Maui.Controls.resources.dll => 0x47b79c15 => 20
	i32 1234928153, ; 72: nb/Microsoft.Maui.Controls.resources.dll => 0x499b8219 => 18
	i32 1260983243, ; 73: cs\Microsoft.Maui.Controls.resources => 0x4b2913cb => 2
	i32 1293217323, ; 74: Xamarin.AndroidX.DrawerLayout.dll => 0x4d14ee2b => 74
	i32 1324164729, ; 75: System.Linq => 0x4eed2679 => 111
	i32 1373134921, ; 76: zh-Hans\Microsoft.Maui.Controls.resources => 0x51d86049 => 32
	i32 1376866003, ; 77: Xamarin.AndroidX.SavedState => 0x52114ed3 => 86
	i32 1406073936, ; 78: Xamarin.AndroidX.CoordinatorLayout => 0x53cefc50 => 70
	i32 1430672901, ; 79: ar\Microsoft.Maui.Controls.resources => 0x55465605 => 0
	i32 1454105418, ; 80: Microsoft.Extensions.FileProviders.Composite => 0x56abe34a => 48
	i32 1461004990, ; 81: es\Microsoft.Maui.Controls.resources => 0x57152abe => 6
	i32 1461234159, ; 82: System.Collections.Immutable.dll => 0x5718a9ef => 97
	i32 1462112819, ; 83: System.IO.Compression.dll => 0x57261233 => 108
	i32 1469204771, ; 84: Xamarin.AndroidX.AppCompat.AppCompatResources => 0x57924923 => 67
	i32 1470490898, ; 85: Microsoft.Extensions.Primitives => 0x57a5e912 => 57
	i32 1470577258, ; 86: GamifyMe.UI.Shared => 0x57a73a6a => 94
	i32 1479771757, ; 87: System.Collections.Immutable => 0x5833866d => 97
	i32 1480492111, ; 88: System.IO.Compression.Brotli.dll => 0x583e844f => 107
	i32 1493001747, ; 89: hi/Microsoft.Maui.Controls.resources.dll => 0x58fd6613 => 10
	i32 1514721132, ; 90: el/Microsoft.Maui.Controls.resources.dll => 0x5a48cf6c => 5
	i32 1521091094, ; 91: Microsoft.Extensions.FileSystemGlobbing => 0x5aaa0216 => 51
	i32 1543031311, ; 92: System.Text.RegularExpressions.dll => 0x5bf8ca0f => 130
	i32 1546581739, ; 93: Microsoft.AspNetCore.Components.WebView.Maui.dll => 0x5c2ef6eb => 41
	i32 1551623176, ; 94: sk/Microsoft.Maui.Controls.resources.dll => 0x5c7be408 => 25
	i32 1622152042, ; 95: Xamarin.AndroidX.Loader.dll => 0x60b0136a => 80
	i32 1624863272, ; 96: Xamarin.AndroidX.ViewPager2 => 0x60d97228 => 89
	i32 1636350590, ; 97: Xamarin.AndroidX.CursorAdapter => 0x6188ba7e => 72
	i32 1639515021, ; 98: System.Net.Http.dll => 0x61b9038d => 114
	i32 1639986890, ; 99: System.Text.RegularExpressions => 0x61c036ca => 130
	i32 1654881142, ; 100: Microsoft.AspNetCore.Components.WebView => 0x62a37b76 => 40
	i32 1657153582, ; 101: System.Runtime => 0x62c6282e => 125
	i32 1658251792, ; 102: Xamarin.Google.Android.Material.dll => 0x62d6ea10 => 90
	i32 1677501392, ; 103: System.Net.Primitives.dll => 0x63fca3d0 => 115
	i32 1679769178, ; 104: System.Security.Cryptography => 0x641f3e5a => 127
	i32 1729485958, ; 105: Xamarin.AndroidX.CardView.dll => 0x6715dc86 => 68
	i32 1736233607, ; 106: ro/Microsoft.Maui.Controls.resources.dll => 0x677cd287 => 23
	i32 1743415430, ; 107: ca\Microsoft.Maui.Controls.resources => 0x67ea6886 => 1
	i32 1760259689, ; 108: Microsoft.AspNetCore.Components.Web.dll => 0x68eb6e69 => 39
	i32 1766324549, ; 109: Xamarin.AndroidX.SwipeRefreshLayout => 0x6947f945 => 87
	i32 1770582343, ; 110: Microsoft.Extensions.Logging.dll => 0x6988f147 => 54
	i32 1780572499, ; 111: Mono.Android.Runtime.dll => 0x6a216153 => 138
	i32 1782862114, ; 112: ms\Microsoft.Maui.Controls.resources => 0x6a445122 => 17
	i32 1788241197, ; 113: Xamarin.AndroidX.Fragment => 0x6a96652d => 75
	i32 1793755602, ; 114: he\Microsoft.Maui.Controls.resources => 0x6aea89d2 => 9
	i32 1808609942, ; 115: Xamarin.AndroidX.Loader => 0x6bcd3296 => 80
	i32 1813058853, ; 116: Xamarin.Kotlin.StdLib.dll => 0x6c111525 => 91
	i32 1813201214, ; 117: Xamarin.Google.Android.Material => 0x6c13413e => 90
	i32 1818569960, ; 118: Xamarin.AndroidX.Navigation.UI.dll => 0x6c652ce8 => 84
	i32 1828688058, ; 119: Microsoft.Extensions.Logging.Abstractions.dll => 0x6cff90ba => 55
	i32 1842015223, ; 120: uk/Microsoft.Maui.Controls.resources.dll => 0x6dcaebf7 => 29
	i32 1853025655, ; 121: sv\Microsoft.Maui.Controls.resources => 0x6e72ed77 => 26
	i32 1858542181, ; 122: System.Linq.Expressions => 0x6ec71a65 => 110
	i32 1875935024, ; 123: fr\Microsoft.Maui.Controls.resources => 0x6fd07f30 => 8
	i32 1910275211, ; 124: System.Collections.NonGeneric.dll => 0x71dc7c8b => 98
	i32 1939592360, ; 125: System.Private.Xml.Linq => 0x739bd4a8 => 120
	i32 1968388702, ; 126: Microsoft.Extensions.Configuration.dll => 0x75533a5e => 43
	i32 2003115576, ; 127: el\Microsoft.Maui.Controls.resources => 0x77651e38 => 5
	i32 2019465201, ; 128: Xamarin.AndroidX.Lifecycle.ViewModel => 0x785e97f1 => 78
	i32 2025202353, ; 129: ar/Microsoft.Maui.Controls.resources.dll => 0x78b622b1 => 0
	i32 2045470958, ; 130: System.Private.Xml => 0x79eb68ee => 121
	i32 2055257422, ; 131: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 0x7a80bd4e => 77
	i32 2066184531, ; 132: de\Microsoft.Maui.Controls.resources => 0x7b277953 => 4
	i32 2072397586, ; 133: Microsoft.Extensions.FileProviders.Physical => 0x7b864712 => 50
	i32 2079903147, ; 134: System.Runtime.dll => 0x7bf8cdab => 125
	i32 2090596640, ; 135: System.Numerics.Vectors => 0x7c9bf920 => 117
	i32 2127167465, ; 136: System.Console => 0x7ec9ffe9 => 105
	i32 2142473426, ; 137: System.Collections.Specialized => 0x7fb38cd2 => 99
	i32 2159891885, ; 138: Microsoft.Maui => 0x80bd55ad => 61
	i32 2169148018, ; 139: hu\Microsoft.Maui.Controls.resources => 0x814a9272 => 12
	i32 2181898931, ; 140: Microsoft.Extensions.Options.dll => 0x820d22b3 => 56
	i32 2192057212, ; 141: Microsoft.Extensions.Logging.Abstractions => 0x82a8237c => 55
	i32 2192166651, ; 142: Microsoft.AspNetCore.Components.Authorization.dll => 0x82a9cefb => 37
	i32 2193016926, ; 143: System.ObjectModel.dll => 0x82b6c85e => 118
	i32 2201107256, ; 144: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 0x83323b38 => 92
	i32 2201231467, ; 145: System.Net.Http => 0x8334206b => 114
	i32 2207618523, ; 146: it\Microsoft.Maui.Controls.resources => 0x839595db => 14
	i32 2266799131, ; 147: Microsoft.Extensions.Configuration.Abstractions => 0x871c9c1b => 44
	i32 2270573516, ; 148: fr/Microsoft.Maui.Controls.resources.dll => 0x875633cc => 8
	i32 2279755925, ; 149: Xamarin.AndroidX.RecyclerView.dll => 0x87e25095 => 85
	i32 2303942373, ; 150: nb\Microsoft.Maui.Controls.resources => 0x89535ee5 => 18
	i32 2305521784, ; 151: System.Private.CoreLib.dll => 0x896b7878 => 136
	i32 2353062107, ; 152: System.Net.Primitives => 0x8c40e0db => 115
	i32 2368005991, ; 153: System.Xml.ReaderWriter.dll => 0x8d24e767 => 134
	i32 2371007202, ; 154: Microsoft.Extensions.Configuration => 0x8d52b2e2 => 43
	i32 2395872292, ; 155: id\Microsoft.Maui.Controls.resources => 0x8ece1c24 => 13
	i32 2411328690, ; 156: Microsoft.AspNetCore.Components => 0x8fb9f4b2 => 36
	i32 2427813419, ; 157: hi\Microsoft.Maui.Controls.resources => 0x90b57e2b => 10
	i32 2435356389, ; 158: System.Console.dll => 0x912896e5 => 105
	i32 2442556106, ; 159: Microsoft.JSInterop.dll => 0x919672ca => 58
	i32 2475788418, ; 160: Java.Interop.dll => 0x93918882 => 137
	i32 2480646305, ; 161: Microsoft.Maui.Controls => 0x93dba8a1 => 59
	i32 2537015816, ; 162: Microsoft.AspNetCore.Authorization => 0x9737ca08 => 35
	i32 2550873716, ; 163: hr\Microsoft.Maui.Controls.resources => 0x980b3e74 => 11
	i32 2570120770, ; 164: System.Text.Encodings.Web => 0x9930ee42 => 128
	i32 2585813321, ; 165: Microsoft.AspNetCore.Components.Forms => 0x9a206149 => 38
	i32 2592341985, ; 166: Microsoft.Extensions.FileProviders.Abstractions => 0x9a83ffe1 => 47
	i32 2593496499, ; 167: pl\Microsoft.Maui.Controls.resources => 0x9a959db3 => 20
	i32 2605712449, ; 168: Xamarin.KotlinX.Coroutines.Core.Jvm => 0x9b500441 => 92
	i32 2617129537, ; 169: System.Private.Xml.dll => 0x9bfe3a41 => 121
	i32 2620871830, ; 170: Xamarin.AndroidX.CursorAdapter.dll => 0x9c375496 => 72
	i32 2626831493, ; 171: ja\Microsoft.Maui.Controls.resources => 0x9c924485 => 15
	i32 2663698177, ; 172: System.Runtime.Loader => 0x9ec4cf01 => 123
	i32 2692077919, ; 173: Microsoft.AspNetCore.Components.WebView.dll => 0xa075d95f => 40
	i32 2717744543, ; 174: System.Security.Claims => 0xa1fd7d9f => 126
	i32 2732626843, ; 175: Xamarin.AndroidX.Activity => 0xa2e0939b => 65
	i32 2735631878, ; 176: Microsoft.AspNetCore.Authorization.dll => 0xa30e6e06 => 35
	i32 2737747696, ; 177: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 0xa32eb6f0 => 67
	i32 2752995522, ; 178: pt-BR\Microsoft.Maui.Controls.resources => 0xa41760c2 => 21
	i32 2758225723, ; 179: Microsoft.Maui.Controls.Xaml => 0xa4672f3b => 60
	i32 2764765095, ; 180: Microsoft.Maui.dll => 0xa4caf7a7 => 61
	i32 2778768386, ; 181: Xamarin.AndroidX.ViewPager.dll => 0xa5a0a402 => 88
	i32 2785988530, ; 182: th\Microsoft.Maui.Controls.resources => 0xa60ecfb2 => 27
	i32 2801831435, ; 183: Microsoft.Maui.Graphics => 0xa7008e0b => 63
	i32 2806116107, ; 184: es/Microsoft.Maui.Controls.resources.dll => 0xa741ef0b => 6
	i32 2810250172, ; 185: Xamarin.AndroidX.CoordinatorLayout.dll => 0xa78103bc => 70
	i32 2820942282, ; 186: MudBlazor.dll => 0xa82429ca => 64
	i32 2831556043, ; 187: nl/Microsoft.Maui.Controls.resources.dll => 0xa8c61dcb => 19
	i32 2833784645, ; 188: Microsoft.AspNetCore.Metadata => 0xa8e81f45 => 42
	i32 2853208004, ; 189: Xamarin.AndroidX.ViewPager => 0xaa107fc4 => 88
	i32 2861189240, ; 190: Microsoft.Maui.Essentials => 0xaa8a4878 => 62
	i32 2892341533, ; 191: Microsoft.AspNetCore.Components.Web => 0xac65a11d => 39
	i32 2909740682, ; 192: System.Private.CoreLib => 0xad6f1e8a => 136
	i32 2911054922, ; 193: Microsoft.Extensions.FileProviders.Physical.dll => 0xad832c4a => 50
	i32 2916838712, ; 194: Xamarin.AndroidX.ViewPager2.dll => 0xaddb6d38 => 89
	i32 2919462931, ; 195: System.Numerics.Vectors.dll => 0xae037813 => 117
	i32 2959614098, ; 196: System.ComponentModel.dll => 0xb0682092 => 104
	i32 2978675010, ; 197: Xamarin.AndroidX.DrawerLayout => 0xb18af942 => 74
	i32 3038032645, ; 198: _Microsoft.Android.Resource.Designer.dll => 0xb514b305 => 34
	i32 3057625584, ; 199: Xamarin.AndroidX.Navigation.Common => 0xb63fa9f0 => 81
	i32 3059408633, ; 200: Mono.Android.Runtime => 0xb65adef9 => 138
	i32 3059793426, ; 201: System.ComponentModel.Primitives => 0xb660be12 => 102
	i32 3060069052, ; 202: MudBlazor => 0xb664f2bc => 64
	i32 3077302341, ; 203: hu/Microsoft.Maui.Controls.resources.dll => 0xb76be845 => 12
	i32 3099732863, ; 204: System.Security.Claims.dll => 0xb8c22b7f => 126
	i32 3178803400, ; 205: Xamarin.AndroidX.Navigation.Fragment.dll => 0xbd78b0c8 => 82
	i32 3220365878, ; 206: System.Threading => 0xbff2e236 => 132
	i32 3258312781, ; 207: Xamarin.AndroidX.CardView => 0xc235e84d => 68
	i32 3280506390, ; 208: System.ComponentModel.Annotations.dll => 0xc3888e16 => 101
	i32 3305363605, ; 209: fi\Microsoft.Maui.Controls.resources => 0xc503d895 => 7
	i32 3316684772, ; 210: System.Net.Requests.dll => 0xc5b097e4 => 116
	i32 3317135071, ; 211: Xamarin.AndroidX.CustomView.dll => 0xc5b776df => 73
	i32 3346324047, ; 212: Xamarin.AndroidX.Navigation.Runtime => 0xc774da4f => 83
	i32 3357674450, ; 213: ru\Microsoft.Maui.Controls.resources => 0xc8220bd2 => 24
	i32 3358260929, ; 214: System.Text.Json => 0xc82afec1 => 129
	i32 3362522851, ; 215: Xamarin.AndroidX.Core => 0xc86c06e3 => 71
	i32 3366347497, ; 216: Java.Interop => 0xc8a662e9 => 137
	i32 3374999561, ; 217: Xamarin.AndroidX.RecyclerView => 0xc92a6809 => 85
	i32 3381016424, ; 218: da\Microsoft.Maui.Controls.resources => 0xc9863768 => 3
	i32 3406629867, ; 219: Microsoft.Extensions.FileProviders.Composite.dll => 0xcb0d0beb => 48
	i32 3428513518, ; 220: Microsoft.Extensions.DependencyInjection.dll => 0xcc5af6ee => 45
	i32 3463511458, ; 221: hr/Microsoft.Maui.Controls.resources.dll => 0xce70fda2 => 11
	i32 3464190856, ; 222: Microsoft.AspNetCore.Components.Forms.dll => 0xce7b5b88 => 38
	i32 3471940407, ; 223: System.ComponentModel.TypeConverter.dll => 0xcef19b37 => 103
	i32 3476120550, ; 224: Mono.Android => 0xcf3163e6 => 139
	i32 3479583265, ; 225: ru/Microsoft.Maui.Controls.resources.dll => 0xcf663a21 => 24
	i32 3484440000, ; 226: ro\Microsoft.Maui.Controls.resources => 0xcfb055c0 => 23
	i32 3485117614, ; 227: System.Text.Json.dll => 0xcfbaacae => 129
	i32 3500000672, ; 228: Microsoft.JSInterop => 0xd09dc5a0 => 58
	i32 3509114376, ; 229: System.Xml.Linq => 0xd128d608 => 133
	i32 3580758918, ; 230: zh-HK\Microsoft.Maui.Controls.resources => 0xd56e0b86 => 31
	i32 3592435036, ; 231: Microsoft.Extensions.Localization.Abstractions => 0xd620355c => 53
	i32 3608519521, ; 232: System.Linq.dll => 0xd715a361 => 111
	i32 3641597786, ; 233: Xamarin.AndroidX.Lifecycle.LiveData.Core => 0xd90e5f5a => 77
	i32 3643446276, ; 234: tr\Microsoft.Maui.Controls.resources => 0xd92a9404 => 28
	i32 3643854240, ; 235: Xamarin.AndroidX.Navigation.Fragment => 0xd930cda0 => 82
	i32 3657292374, ; 236: Microsoft.Extensions.Configuration.Abstractions.dll => 0xd9fdda56 => 44
	i32 3672681054, ; 237: Mono.Android.dll => 0xdae8aa5e => 139
	i32 3693185972, ; 238: GamifyMe.App.dll => 0xdc218bb4 => 95
	i32 3697841164, ; 239: zh-Hant/Microsoft.Maui.Controls.resources.dll => 0xdc68940c => 33
	i32 3724971120, ; 240: Xamarin.AndroidX.Navigation.Common.dll => 0xde068c70 => 81
	i32 3732214720, ; 241: Microsoft.AspNetCore.Metadata.dll => 0xde7513c0 => 42
	i32 3737834244, ; 242: System.Net.Http.Json.dll => 0xdecad304 => 113
	i32 3748608112, ; 243: System.Diagnostics.DiagnosticSource => 0xdf6f3870 => 106
	i32 3776403777, ; 244: Microsoft.Extensions.Localization.Abstractions.dll => 0xe1175941 => 53
	i32 3786282454, ; 245: Xamarin.AndroidX.Collection => 0xe1ae15d6 => 69
	i32 3792276235, ; 246: System.Collections.NonGeneric => 0xe2098b0b => 98
	i32 3802395368, ; 247: System.Collections.Specialized.dll => 0xe2a3f2e8 => 99
	i32 3823082795, ; 248: System.Security.Cryptography.dll => 0xe3df9d2b => 127
	i32 3841636137, ; 249: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 0xe4fab729 => 46
	i32 3849253459, ; 250: System.Runtime.InteropServices.dll => 0xe56ef253 => 122
	i32 3868809053, ; 251: GamifyMe.Shared => 0xe699575d => 93
	i32 3889960447, ; 252: zh-Hans/Microsoft.Maui.Controls.resources.dll => 0xe7dc15ff => 32
	i32 3896106733, ; 253: System.Collections.Concurrent.dll => 0xe839deed => 96
	i32 3896760992, ; 254: Xamarin.AndroidX.Core.dll => 0xe843daa0 => 71
	i32 3898971068, ; 255: Microsoft.Extensions.Localization.dll => 0xe86593bc => 52
	i32 3928044579, ; 256: System.Xml.ReaderWriter => 0xea213423 => 134
	i32 3931092270, ; 257: Xamarin.AndroidX.Navigation.UI => 0xea4fb52e => 84
	i32 3954195687, ; 258: Microsoft.Extensions.Localization => 0xebb03ce7 => 52
	i32 3955647286, ; 259: Xamarin.AndroidX.AppCompat.dll => 0xebc66336 => 66
	i32 3980434154, ; 260: th/Microsoft.Maui.Controls.resources.dll => 0xed409aea => 27
	i32 3987592930, ; 261: he/Microsoft.Maui.Controls.resources.dll => 0xedadd6e2 => 9
	i32 4025784931, ; 262: System.Memory => 0xeff49a63 => 112
	i32 4046471985, ; 263: Microsoft.Maui.Controls.Xaml.dll => 0xf1304331 => 60
	i32 4068434129, ; 264: System.Private.Xml.Linq.dll => 0xf27f60d1 => 120
	i32 4073602200, ; 265: System.Threading.dll => 0xf2ce3c98 => 132
	i32 4094352644, ; 266: Microsoft.Maui.Essentials.dll => 0xf40add04 => 62
	i32 4100113165, ; 267: System.Private.Uri => 0xf462c30d => 119
	i32 4102112229, ; 268: pt/Microsoft.Maui.Controls.resources.dll => 0xf48143e5 => 22
	i32 4125707920, ; 269: ms/Microsoft.Maui.Controls.resources.dll => 0xf5e94e90 => 17
	i32 4126470640, ; 270: Microsoft.Extensions.DependencyInjection => 0xf5f4f1f0 => 45
	i32 4127667938, ; 271: System.IO.FileSystem.Watcher => 0xf60736e2 => 109
	i32 4150914736, ; 272: uk\Microsoft.Maui.Controls.resources => 0xf769eeb0 => 29
	i32 4164802419, ; 273: System.IO.FileSystem.Watcher.dll => 0xf83dd773 => 109
	i32 4181436372, ; 274: System.Runtime.Serialization.Primitives => 0xf93ba7d4 => 124
	i32 4182413190, ; 275: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 0xf94a8f86 => 79
	i32 4213026141, ; 276: System.Diagnostics.DiagnosticSource.dll => 0xfb1dad5d => 106
	i32 4271975918, ; 277: Microsoft.Maui.Controls.dll => 0xfea12dee => 59
	i32 4292120959, ; 278: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 0xffd4917f => 79
	i32 4294648842 ; 279: Microsoft.Extensions.FileProviders.Embedded => 0xfffb240a => 49
], align 4

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [280 x i32] [
	i32 131, ; 0
	i32 33, ; 1
	i32 63, ; 2
	i32 122, ; 3
	i32 101, ; 4
	i32 69, ; 5
	i32 87, ; 6
	i32 30, ; 7
	i32 31, ; 8
	i32 104, ; 9
	i32 37, ; 10
	i32 95, ; 11
	i32 36, ; 12
	i32 2, ; 13
	i32 30, ; 14
	i32 65, ; 15
	i32 15, ; 16
	i32 76, ; 17
	i32 14, ; 18
	i32 131, ; 19
	i32 112, ; 20
	i32 34, ; 21
	i32 26, ; 22
	i32 100, ; 23
	i32 75, ; 24
	i32 124, ; 25
	i32 135, ; 26
	i32 118, ; 27
	i32 13, ; 28
	i32 7, ; 29
	i32 57, ; 30
	i32 54, ; 31
	i32 49, ; 32
	i32 21, ; 33
	i32 93, ; 34
	i32 73, ; 35
	i32 19, ; 36
	i32 128, ; 37
	i32 96, ; 38
	i32 1, ; 39
	i32 133, ; 40
	i32 16, ; 41
	i32 4, ; 42
	i32 123, ; 43
	i32 116, ; 44
	i32 108, ; 45
	i32 25, ; 46
	i32 56, ; 47
	i32 41, ; 48
	i32 119, ; 49
	i32 107, ; 50
	i32 113, ; 51
	i32 102, ; 52
	i32 28, ; 53
	i32 76, ; 54
	i32 100, ; 55
	i32 51, ; 56
	i32 86, ; 57
	i32 46, ; 58
	i32 3, ; 59
	i32 66, ; 60
	i32 110, ; 61
	i32 78, ; 62
	i32 103, ; 63
	i32 91, ; 64
	i32 135, ; 65
	i32 16, ; 66
	i32 94, ; 67
	i32 22, ; 68
	i32 47, ; 69
	i32 83, ; 70
	i32 20, ; 71
	i32 18, ; 72
	i32 2, ; 73
	i32 74, ; 74
	i32 111, ; 75
	i32 32, ; 76
	i32 86, ; 77
	i32 70, ; 78
	i32 0, ; 79
	i32 48, ; 80
	i32 6, ; 81
	i32 97, ; 82
	i32 108, ; 83
	i32 67, ; 84
	i32 57, ; 85
	i32 94, ; 86
	i32 97, ; 87
	i32 107, ; 88
	i32 10, ; 89
	i32 5, ; 90
	i32 51, ; 91
	i32 130, ; 92
	i32 41, ; 93
	i32 25, ; 94
	i32 80, ; 95
	i32 89, ; 96
	i32 72, ; 97
	i32 114, ; 98
	i32 130, ; 99
	i32 40, ; 100
	i32 125, ; 101
	i32 90, ; 102
	i32 115, ; 103
	i32 127, ; 104
	i32 68, ; 105
	i32 23, ; 106
	i32 1, ; 107
	i32 39, ; 108
	i32 87, ; 109
	i32 54, ; 110
	i32 138, ; 111
	i32 17, ; 112
	i32 75, ; 113
	i32 9, ; 114
	i32 80, ; 115
	i32 91, ; 116
	i32 90, ; 117
	i32 84, ; 118
	i32 55, ; 119
	i32 29, ; 120
	i32 26, ; 121
	i32 110, ; 122
	i32 8, ; 123
	i32 98, ; 124
	i32 120, ; 125
	i32 43, ; 126
	i32 5, ; 127
	i32 78, ; 128
	i32 0, ; 129
	i32 121, ; 130
	i32 77, ; 131
	i32 4, ; 132
	i32 50, ; 133
	i32 125, ; 134
	i32 117, ; 135
	i32 105, ; 136
	i32 99, ; 137
	i32 61, ; 138
	i32 12, ; 139
	i32 56, ; 140
	i32 55, ; 141
	i32 37, ; 142
	i32 118, ; 143
	i32 92, ; 144
	i32 114, ; 145
	i32 14, ; 146
	i32 44, ; 147
	i32 8, ; 148
	i32 85, ; 149
	i32 18, ; 150
	i32 136, ; 151
	i32 115, ; 152
	i32 134, ; 153
	i32 43, ; 154
	i32 13, ; 155
	i32 36, ; 156
	i32 10, ; 157
	i32 105, ; 158
	i32 58, ; 159
	i32 137, ; 160
	i32 59, ; 161
	i32 35, ; 162
	i32 11, ; 163
	i32 128, ; 164
	i32 38, ; 165
	i32 47, ; 166
	i32 20, ; 167
	i32 92, ; 168
	i32 121, ; 169
	i32 72, ; 170
	i32 15, ; 171
	i32 123, ; 172
	i32 40, ; 173
	i32 126, ; 174
	i32 65, ; 175
	i32 35, ; 176
	i32 67, ; 177
	i32 21, ; 178
	i32 60, ; 179
	i32 61, ; 180
	i32 88, ; 181
	i32 27, ; 182
	i32 63, ; 183
	i32 6, ; 184
	i32 70, ; 185
	i32 64, ; 186
	i32 19, ; 187
	i32 42, ; 188
	i32 88, ; 189
	i32 62, ; 190
	i32 39, ; 191
	i32 136, ; 192
	i32 50, ; 193
	i32 89, ; 194
	i32 117, ; 195
	i32 104, ; 196
	i32 74, ; 197
	i32 34, ; 198
	i32 81, ; 199
	i32 138, ; 200
	i32 102, ; 201
	i32 64, ; 202
	i32 12, ; 203
	i32 126, ; 204
	i32 82, ; 205
	i32 132, ; 206
	i32 68, ; 207
	i32 101, ; 208
	i32 7, ; 209
	i32 116, ; 210
	i32 73, ; 211
	i32 83, ; 212
	i32 24, ; 213
	i32 129, ; 214
	i32 71, ; 215
	i32 137, ; 216
	i32 85, ; 217
	i32 3, ; 218
	i32 48, ; 219
	i32 45, ; 220
	i32 11, ; 221
	i32 38, ; 222
	i32 103, ; 223
	i32 139, ; 224
	i32 24, ; 225
	i32 23, ; 226
	i32 129, ; 227
	i32 58, ; 228
	i32 133, ; 229
	i32 31, ; 230
	i32 53, ; 231
	i32 111, ; 232
	i32 77, ; 233
	i32 28, ; 234
	i32 82, ; 235
	i32 44, ; 236
	i32 139, ; 237
	i32 95, ; 238
	i32 33, ; 239
	i32 81, ; 240
	i32 42, ; 241
	i32 113, ; 242
	i32 106, ; 243
	i32 53, ; 244
	i32 69, ; 245
	i32 98, ; 246
	i32 99, ; 247
	i32 127, ; 248
	i32 46, ; 249
	i32 122, ; 250
	i32 93, ; 251
	i32 32, ; 252
	i32 96, ; 253
	i32 71, ; 254
	i32 52, ; 255
	i32 134, ; 256
	i32 84, ; 257
	i32 52, ; 258
	i32 66, ; 259
	i32 27, ; 260
	i32 9, ; 261
	i32 112, ; 262
	i32 60, ; 263
	i32 120, ; 264
	i32 132, ; 265
	i32 62, ; 266
	i32 119, ; 267
	i32 22, ; 268
	i32 17, ; 269
	i32 45, ; 270
	i32 109, ; 271
	i32 29, ; 272
	i32 109, ; 273
	i32 124, ; 274
	i32 79, ; 275
	i32 106, ; 276
	i32 59, ; 277
	i32 79, ; 278
	i32 49 ; 279
], align 4

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 0, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [0 x %struct.MarshalMethodsManagedClass] zeroinitializer, align 4

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [0 x ptr] zeroinitializer, align 4

@mm_method_names = dso_local local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 0, ; id 0x0; name: 
		ptr @.MarshalMethodName.0_name; char* name
	} ; 0
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 4

; Functions

; Function attributes: "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" uwtable willreturn
define void @xamarin_app_init(ptr nocapture noundef readnone %env, ptr noundef %fn) local_unnamed_addr #0
{
	%fnIsNull = icmp eq ptr %fn, null
	br i1 %fnIsNull, label %1, label %2

1: ; preds = %0
	%putsResult = call noundef i32 @puts(ptr @.str.0)
	call void @abort()
	unreachable 

2: ; preds = %1, %0
	store ptr %fn, ptr @get_function_pointer, align 4, !tbaa !3
	ret void
}

; Strings
@.str.0 = private unnamed_addr constant [40 x i8] c"get_function_pointer MUST be specified\0A\00", align 1

;MarshalMethodName
@.MarshalMethodName.0_name = private unnamed_addr constant [1 x i8] c"\00", align 1

; External functions

; Function attributes: noreturn "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+armv7-a,+d32,+dsp,+fp64,+neon,+vfp2,+vfp2sp,+vfp3,+vfp3d16,+vfp3d16sp,+vfp3sp,-aes,-fp-armv8,-fp-armv8d16,-fp-armv8d16sp,-fp-armv8sp,-fp16,-fp16fml,-fullfp16,-sha2,-thumb-mode,-vfp4,-vfp4d16,-vfp4d16sp,-vfp4sp" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { noreturn "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+armv7-a,+d32,+dsp,+fp64,+neon,+vfp2,+vfp2sp,+vfp3,+vfp3d16,+vfp3d16sp,+vfp3sp,-aes,-fp-armv8,-fp-armv8d16,-fp-armv8d16sp,-fp-armv8sp,-fp16,-fp16fml,-fullfp16,-sha2,-thumb-mode,-vfp4,-vfp4d16,-vfp4d16sp,-vfp4sp" }

; Metadata
!llvm.module.flags = !{!0, !1, !7}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!"Xamarin.Android remotes/origin/release/8.0.4xx @ 82d8938cf80f6d5fa6c28529ddfbdb753d805ab4"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"min_enum_size", i32 4}
