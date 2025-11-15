; ModuleID = 'marshal_methods.x86_64.ll'
source_filename = "marshal_methods.x86_64.ll"
target datalayout = "e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
target triple = "x86_64-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [140 x ptr] zeroinitializer, align 16

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [280 x i64] [
	i64 98382396393917666, ; 0: Microsoft.Extensions.Primitives.dll => 0x15d8644ad360ce2 => 57
	i64 120698629574877762, ; 1: Mono.Android => 0x1accec39cafe242 => 139
	i64 131669012237370309, ; 2: Microsoft.Maui.Essentials.dll => 0x1d3c844de55c3c5 => 62
	i64 196720943101637631, ; 3: System.Linq.Expressions.dll => 0x2bae4a7cd73f3ff => 110
	i64 210515253464952879, ; 4: Xamarin.AndroidX.Collection.dll => 0x2ebe681f694702f => 69
	i64 232391251801502327, ; 5: Xamarin.AndroidX.SavedState.dll => 0x3399e9cbc897277 => 86
	i64 544876430810592561, ; 6: GamifyMe.App => 0x78fca3a18f2b531 => 95
	i64 545109961164950392, ; 7: fi/Microsoft.Maui.Controls.resources.dll => 0x7909e9f1ec38b78 => 7
	i64 683390398661839228, ; 8: Microsoft.Extensions.FileProviders.Embedded => 0x97be3f26326c97c => 49
	i64 750875890346172408, ; 9: System.Threading.Thread => 0xa6ba5a4da7d1ff8 => 131
	i64 799765834175365804, ; 10: System.ComponentModel.dll => 0xb1956c9f18442ac => 104
	i64 849051935479314978, ; 11: hi/Microsoft.Maui.Controls.resources.dll => 0xbc8703ca21a3a22 => 10
	i64 872800313462103108, ; 12: Xamarin.AndroidX.DrawerLayout => 0xc1ccf42c3c21c44 => 74
	i64 1120440138749646132, ; 13: Xamarin.Google.Android.Material.dll => 0xf8c9a5eae431534 => 90
	i64 1121665720830085036, ; 14: nb/Microsoft.Maui.Controls.resources.dll => 0xf90f507becf47ac => 18
	i64 1369545283391376210, ; 15: Xamarin.AndroidX.Navigation.Fragment.dll => 0x13019a2dd85acb52 => 82
	i64 1404195534211153682, ; 16: System.IO.FileSystem.Watcher.dll => 0x137cb4660bd87f12 => 109
	i64 1476839205573959279, ; 17: System.Net.Primitives.dll => 0x147ec96ece9b1e6f => 115
	i64 1486715745332614827, ; 18: Microsoft.Maui.Controls.dll => 0x14a1e017ea87d6ab => 59
	i64 1513467482682125403, ; 19: Mono.Android.Runtime => 0x1500eaa8245f6c5b => 138
	i64 1537168428375924959, ; 20: System.Threading.Thread.dll => 0x15551e8a954ae0df => 131
	i64 1556147632182429976, ; 21: ko/Microsoft.Maui.Controls.resources.dll => 0x15988c06d24c8918 => 16
	i64 1624659445732251991, ; 22: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 0x168bf32877da9957 => 67
	i64 1628611045998245443, ; 23: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 0x1699fd1e1a00b643 => 79
	i64 1743969030606105336, ; 24: System.Memory.dll => 0x1833d297e88f2af8 => 112
	i64 1767386781656293639, ; 25: System.Private.Uri.dll => 0x188704e9f5582107 => 119
	i64 1776954265264967804, ; 26: Microsoft.JSInterop.dll => 0x18a9027d533bd07c => 58
	i64 1795316252682057001, ; 27: Xamarin.AndroidX.AppCompat.dll => 0x18ea3e9eac997529 => 66
	i64 1835311033149317475, ; 28: es\Microsoft.Maui.Controls.resources => 0x197855a927386163 => 6
	i64 1836611346387731153, ; 29: Xamarin.AndroidX.SavedState => 0x197cf449ebe482d1 => 86
	i64 1881198190668717030, ; 30: tr\Microsoft.Maui.Controls.resources => 0x1a1b5bc992ea9be6 => 28
	i64 1920760634179481754, ; 31: Microsoft.Maui.Controls.Xaml => 0x1aa7e99ec2d2709a => 60
	i64 1959996714666907089, ; 32: tr/Microsoft.Maui.Controls.resources.dll => 0x1b334ea0a2a755d1 => 28
	i64 1981742497975770890, ; 33: Xamarin.AndroidX.Lifecycle.ViewModel.dll => 0x1b80904d5c241f0a => 78
	i64 1983698669889758782, ; 34: cs/Microsoft.Maui.Controls.resources.dll => 0x1b87836e2031a63e => 2
	i64 2019660174692588140, ; 35: pl/Microsoft.Maui.Controls.resources.dll => 0x1c07463a6f8e1a6c => 20
	i64 2262844636196693701, ; 36: Xamarin.AndroidX.DrawerLayout.dll => 0x1f673d352266e6c5 => 74
	i64 2287834202362508563, ; 37: System.Collections.Concurrent => 0x1fc00515e8ce7513 => 96
	i64 2295368378960711535, ; 38: Microsoft.AspNetCore.Components.WebView.Maui.dll => 0x1fdac961189e0f6f => 41
	i64 2302323944321350744, ; 39: ru/Microsoft.Maui.Controls.resources.dll => 0x1ff37f6ddb267c58 => 24
	i64 2329709569556905518, ; 40: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 0x2054ca829b447e2e => 77
	i64 2335503487726329082, ; 41: System.Text.Encodings.Web => 0x2069600c4d9d1cfa => 128
	i64 2470498323731680442, ; 42: Xamarin.AndroidX.CoordinatorLayout => 0x2248f922dc398cba => 70
	i64 2497223385847772520, ; 43: System.Runtime => 0x22a7eb7046413568 => 125
	i64 2547086958574651984, ; 44: Xamarin.AndroidX.Activity.dll => 0x2359121801df4a50 => 65
	i64 2602673633151553063, ; 45: th\Microsoft.Maui.Controls.resources => 0x241e8de13a460e27 => 27
	i64 2656907746661064104, ; 46: Microsoft.Extensions.DependencyInjection => 0x24df3b84c8b75da8 => 45
	i64 2662981627730767622, ; 47: cs\Microsoft.Maui.Controls.resources => 0x24f4cfae6c48af06 => 2
	i64 2781169761569919449, ; 48: Microsoft.JSInterop => 0x2698b329b26ed1d9 => 58
	i64 2816684743911174413, ; 49: GamifyMe.UI.Shared.dll => 0x2716dfd9e516710d => 94
	i64 2895129759130297543, ; 50: fi\Microsoft.Maui.Controls.resources => 0x282d912d479fa4c7 => 7
	i64 3017704767998173186, ; 51: Xamarin.Google.Android.Material => 0x29e10a7f7d88a002 => 90
	i64 3266690593535380875, ; 52: Microsoft.AspNetCore.Authorization => 0x2d559dc982c94d8b => 35
	i64 3289520064315143713, ; 53: Xamarin.AndroidX.Lifecycle.Common => 0x2da6b911e3063621 => 76
	i64 3311221304742556517, ; 54: System.Numerics.Vectors.dll => 0x2df3d23ba9e2b365 => 117
	i64 3344514922410554693, ; 55: Xamarin.KotlinX.Coroutines.Core.Jvm => 0x2e6a1a9a18463545 => 92
	i64 3396143930648122816, ; 56: Microsoft.Extensions.FileProviders.Abstractions => 0x2f2186e9506155c0 => 47
	i64 3429672777697402584, ; 57: Microsoft.Maui.Essentials => 0x2f98a5385a7b1ed8 => 62
	i64 3494946837667399002, ; 58: Microsoft.Extensions.Configuration => 0x30808ba1c00a455a => 43
	i64 3522470458906976663, ; 59: Xamarin.AndroidX.SwipeRefreshLayout => 0x30e2543832f52197 => 87
	i64 3551103847008531295, ; 60: System.Private.CoreLib.dll => 0x31480e226177735f => 136
	i64 3567343442040498961, ; 61: pt\Microsoft.Maui.Controls.resources => 0x3181bff5bea4ab11 => 22
	i64 3571415421602489686, ; 62: System.Runtime.dll => 0x319037675df7e556 => 125
	i64 3638003163729360188, ; 63: Microsoft.Extensions.Configuration.Abstractions => 0x327cc89a39d5f53c => 44
	i64 3647754201059316852, ; 64: System.Xml.ReaderWriter => 0x329f6d1e86145474 => 134
	i64 3655542548057982301, ; 65: Microsoft.Extensions.Configuration.dll => 0x32bb18945e52855d => 43
	i64 3727469159507183293, ; 66: Xamarin.AndroidX.RecyclerView => 0x33baa1739ba646bd => 85
	i64 3753897248517198740, ; 67: Microsoft.AspNetCore.Components.WebView.dll => 0x341885a8952f1394 => 40
	i64 3794322307918838949, ; 68: Microsoft.AspNetCore.Metadata.dll => 0x34a824092ed7bca5 => 42
	i64 3869221888984012293, ; 69: Microsoft.Extensions.Logging.dll => 0x35b23cceda0ed605 => 54
	i64 3889433610606858880, ; 70: Microsoft.Extensions.FileProviders.Physical.dll => 0x35fa0b4301afd280 => 50
	i64 3890352374528606784, ; 71: Microsoft.Maui.Controls.Xaml.dll => 0x35fd4edf66e00240 => 60
	i64 3933965368022646939, ; 72: System.Net.Requests => 0x369840a8bfadc09b => 116
	i64 3966267475168208030, ; 73: System.Memory => 0x370b03412596249e => 112
	i64 4009997192427317104, ; 74: System.Runtime.Serialization.Primitives => 0x37a65f335cf1a770 => 124
	i64 4073500526318903918, ; 75: System.Private.Xml.dll => 0x3887fb25779ae26e => 121
	i64 4120493066591692148, ; 76: zh-Hant\Microsoft.Maui.Controls.resources => 0x392eee9cdda86574 => 33
	i64 4154383907710350974, ; 77: System.ComponentModel => 0x39a7562737acb67e => 104
	i64 4168469861834746866, ; 78: System.Security.Claims.dll => 0x39d96140fb94ebf2 => 126
	i64 4187479170553454871, ; 79: System.Linq.Expressions => 0x3a1cea1e912fa117 => 110
	i64 4205801962323029395, ; 80: System.ComponentModel.TypeConverter => 0x3a5e0299f7e7ad93 => 103
	i64 4356591372459378815, ; 81: vi/Microsoft.Maui.Controls.resources.dll => 0x3c75b8c562f9087f => 30
	i64 4384840217421645357, ; 82: Microsoft.AspNetCore.Components.Forms => 0x3cda14f22443862d => 38
	i64 4672453897036726049, ; 83: System.IO.FileSystem.Watcher => 0x40d7e4104a437f21 => 109
	i64 4679594760078841447, ; 84: ar/Microsoft.Maui.Controls.resources.dll => 0x40f142a407475667 => 0
	i64 4743821336939966868, ; 85: System.ComponentModel.Annotations => 0x41d5705f4239b194 => 101
	i64 4794310189461587505, ; 86: Xamarin.AndroidX.Activity => 0x4288cfb749e4c631 => 65
	i64 4795410492532947900, ; 87: Xamarin.AndroidX.SwipeRefreshLayout.dll => 0x428cb86f8f9b7bbc => 87
	i64 4853321196694829351, ; 88: System.Runtime.Loader.dll => 0x435a75ea15de7927 => 123
	i64 5103417709280584325, ; 89: System.Collections.Specialized => 0x46d2fb5e161b6285 => 99
	i64 5182934613077526976, ; 90: System.Collections.Specialized.dll => 0x47ed7b91fa9009c0 => 99
	i64 5197073077358930460, ; 91: Microsoft.AspNetCore.Components.Web.dll => 0x481fb66db7b9aa1c => 39
	i64 5290786973231294105, ; 92: System.Runtime.Loader => 0x496ca6b869b72699 => 123
	i64 5471532531798518949, ; 93: sv\Microsoft.Maui.Controls.resources => 0x4beec9d926d82ca5 => 26
	i64 5522859530602327440, ; 94: uk\Microsoft.Maui.Controls.resources => 0x4ca5237b51eead90 => 29
	i64 5570799893513421663, ; 95: System.IO.Compression.Brotli => 0x4d4f74fcdfa6c35f => 107
	i64 5573260873512690141, ; 96: System.Security.Cryptography.dll => 0x4d58333c6e4ea1dd => 127
	i64 5692067934154308417, ; 97: Xamarin.AndroidX.ViewPager2.dll => 0x4efe49a0d4a8bb41 => 89
	i64 5741990095351817038, ; 98: Microsoft.Extensions.Localization.Abstractions.dll => 0x4fafa591c141a34e => 53
	i64 6068057819846744445, ; 99: ro/Microsoft.Maui.Controls.resources.dll => 0x5436126fec7f197d => 23
	i64 6182525717148725503, ; 100: Microsoft.AspNetCore.Components.Authorization => 0x55ccbe62215df0ff => 37
	i64 6200764641006662125, ; 101: ro\Microsoft.Maui.Controls.resources => 0x560d8a96830131ed => 23
	i64 6222399776351216807, ; 102: System.Text.Json.dll => 0x565a67a0ffe264a7 => 129
	i64 6357457916754632952, ; 103: _Microsoft.Android.Resource.Designer => 0x583a3a4ac2a7a0f8 => 34
	i64 6401687960814735282, ; 104: Xamarin.AndroidX.Lifecycle.LiveData.Core => 0x58d75d486341cfb2 => 77
	i64 6478287442656530074, ; 105: hr\Microsoft.Maui.Controls.resources => 0x59e7801b0c6a8e9a => 11
	i64 6548213210057960872, ; 106: Xamarin.AndroidX.CustomView.dll => 0x5adfed387b066da8 => 73
	i64 6560151584539558821, ; 107: Microsoft.Extensions.Options => 0x5b0a571be53243a5 => 56
	i64 6743165466166707109, ; 108: nl\Microsoft.Maui.Controls.resources => 0x5d948943c08c43a5 => 19
	i64 6777482997383978746, ; 109: pt/Microsoft.Maui.Controls.resources.dll => 0x5e0e74e0a2525efa => 22
	i64 6876862101832370452, ; 110: System.Xml.Linq => 0x5f6f85a57d108914 => 133
	i64 6894844156784520562, ; 111: System.Numerics.Vectors => 0x5faf683aead1ad72 => 117
	i64 6920570528939222495, ; 112: Microsoft.AspNetCore.Components.WebView => 0x600ace3ab475a5df => 40
	i64 7083547580668757502, ; 113: System.Private.Xml.Linq.dll => 0x624dd0fe8f56c5fe => 120
	i64 7220009545223068405, ; 114: sv/Microsoft.Maui.Controls.resources.dll => 0x6432a06d99f35af5 => 26
	i64 7270811800166795866, ; 115: System.Linq => 0x64e71ccf51a90a5a => 111
	i64 7377312882064240630, ; 116: System.ComponentModel.TypeConverter.dll => 0x66617afac45a2ff6 => 103
	i64 7488575175965059935, ; 117: System.Xml.Linq.dll => 0x67ecc3724534ab5f => 133
	i64 7489048572193775167, ; 118: System.ObjectModel => 0x67ee71ff6b419e3f => 118
	i64 7654504624184590948, ; 119: System.Net.Http => 0x6a3a4366801b8264 => 114
	i64 7708790323521193081, ; 120: ms/Microsoft.Maui.Controls.resources.dll => 0x6afb1ff4d1730479 => 17
	i64 7714652370974252055, ; 121: System.Private.CoreLib => 0x6b0ff375198b9c17 => 136
	i64 7735352534559001595, ; 122: Xamarin.Kotlin.StdLib.dll => 0x6b597e2582ce8bfb => 91
	i64 7836164640616011524, ; 123: Xamarin.AndroidX.AppCompat.AppCompatResources => 0x6cbfa6390d64d704 => 67
	i64 7939716343021325499, ; 124: MudBlazor.dll => 0x6e2f89f293264cbb => 64
	i64 8014722069583580780, ; 125: Microsoft.AspNetCore.Components.Forms.dll => 0x6f3a03422b034e6c => 38
	i64 8064050204834738623, ; 126: System.Collections.dll => 0x6fe942efa61731bf => 100
	i64 8083354569033831015, ; 127: Xamarin.AndroidX.Lifecycle.Common.dll => 0x702dd82730cad267 => 76
	i64 8085230611270010360, ; 128: System.Net.Http.Json.dll => 0x703482674fdd05f8 => 113
	i64 8087206902342787202, ; 129: System.Diagnostics.DiagnosticSource => 0x703b87d46f3aa082 => 106
	i64 8167236081217502503, ; 130: Java.Interop.dll => 0x7157d9f1a9b8fd27 => 137
	i64 8185542183669246576, ; 131: System.Collections => 0x7198e33f4794aa70 => 100
	i64 8246048515196606205, ; 132: Microsoft.Maui.Graphics.dll => 0x726fd96f64ee56fd => 63
	i64 8368701292315763008, ; 133: System.Security.Cryptography => 0x7423997c6fd56140 => 127
	i64 8400357532724379117, ; 134: Xamarin.AndroidX.Navigation.UI.dll => 0x749410ab44503ded => 84
	i64 8518412311883997971, ; 135: System.Collections.Immutable => 0x76377add7c28e313 => 97
	i64 8563666267364444763, ; 136: System.Private.Uri => 0x76d841191140ca5b => 119
	i64 8614108721271900878, ; 137: pt-BR/Microsoft.Maui.Controls.resources.dll => 0x778b763e14018ace => 21
	i64 8626175481042262068, ; 138: Java.Interop => 0x77b654e585b55834 => 137
	i64 8639588376636138208, ; 139: Xamarin.AndroidX.Navigation.Runtime => 0x77e5fbdaa2fda2e0 => 83
	i64 8677882282824630478, ; 140: pt-BR\Microsoft.Maui.Controls.resources => 0x786e07f5766b00ce => 21
	i64 8725526185868997716, ; 141: System.Diagnostics.DiagnosticSource.dll => 0x79174bd613173454 => 106
	i64 9045785047181495996, ; 142: zh-HK\Microsoft.Maui.Controls.resources => 0x7d891592e3cb0ebc => 31
	i64 9312692141327339315, ; 143: Xamarin.AndroidX.ViewPager2 => 0x813d54296a634f33 => 89
	i64 9324707631942237306, ; 144: Xamarin.AndroidX.AppCompat => 0x8168042fd44a7c7a => 66
	i64 9659729154652888475, ; 145: System.Text.RegularExpressions => 0x860e407c9991dd9b => 130
	i64 9678050649315576968, ; 146: Xamarin.AndroidX.CoordinatorLayout.dll => 0x864f57c9feb18c88 => 70
	i64 9702891218465930390, ; 147: System.Collections.NonGeneric.dll => 0x86a79827b2eb3c96 => 98
	i64 9808709177481450983, ; 148: Mono.Android.dll => 0x881f890734e555e7 => 139
	i64 9956195530459977388, ; 149: Microsoft.Maui => 0x8a2b8315b36616ac => 61
	i64 9991543690424095600, ; 150: es/Microsoft.Maui.Controls.resources.dll => 0x8aa9180c89861370 => 6
	i64 10038780035334861115, ; 151: System.Net.Http.dll => 0x8b50e941206af13b => 114
	i64 10051358222726253779, ; 152: System.Private.Xml => 0x8b7d990c97ccccd3 => 121
	i64 10092835686693276772, ; 153: Microsoft.Maui.Controls => 0x8c10f49539bd0c64 => 59
	i64 10143853363526200146, ; 154: da\Microsoft.Maui.Controls.resources => 0x8cc634e3c2a16b52 => 3
	i64 10229024438826829339, ; 155: Xamarin.AndroidX.CustomView => 0x8df4cb880b10061b => 73
	i64 10246306146906071878, ; 156: GamifyMe.UI.Shared => 0x8e323127423ce346 => 94
	i64 10406448008575299332, ; 157: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 0x906b2153fcb3af04 => 92
	i64 10430153318873392755, ; 158: Xamarin.AndroidX.Core => 0x90bf592ea44f6673 => 71
	i64 10506226065143327199, ; 159: ca\Microsoft.Maui.Controls.resources => 0x91cd9cf11ed169df => 1
	i64 10668175129496804104, ; 160: GamifyMe.Shared.dll => 0x940cf8c203505b08 => 93
	i64 10734191584620811116, ; 161: Microsoft.Extensions.FileProviders.Embedded.dll => 0x94f7825fc04f936c => 49
	i64 10785150219063592792, ; 162: System.Net.Primitives => 0x95ac8cfb68830758 => 115
	i64 11002576679268595294, ; 163: Microsoft.Extensions.Logging.Abstractions => 0x98b1013215cd365e => 55
	i64 11009005086950030778, ; 164: Microsoft.Maui.dll => 0x98c7d7cc621ffdba => 61
	i64 11051904132540108364, ; 165: Microsoft.Extensions.FileProviders.Composite.dll => 0x99604040c7b98e4c => 48
	i64 11103970607964515343, ; 166: hu\Microsoft.Maui.Controls.resources => 0x9a193a6fc41a6c0f => 12
	i64 11162124722117608902, ; 167: Xamarin.AndroidX.ViewPager => 0x9ae7d54b986d05c6 => 88
	i64 11218356222449480316, ; 168: Microsoft.AspNetCore.Components => 0x9baf9b8c02e4f27c => 36
	i64 11220793807500858938, ; 169: ja\Microsoft.Maui.Controls.resources => 0x9bb8448481fdd63a => 15
	i64 11226290749488709958, ; 170: Microsoft.Extensions.Options.dll => 0x9bcbcbf50c874146 => 56
	i64 11340910727871153756, ; 171: Xamarin.AndroidX.CursorAdapter => 0x9d630238642d465c => 72
	i64 11485890710487134646, ; 172: System.Runtime.InteropServices => 0x9f6614bf0f8b71b6 => 122
	i64 11518296021396496455, ; 173: id\Microsoft.Maui.Controls.resources => 0x9fd9353475222047 => 13
	i64 11529969570048099689, ; 174: Xamarin.AndroidX.ViewPager.dll => 0xa002ae3c4dc7c569 => 88
	i64 11530571088791430846, ; 175: Microsoft.Extensions.Logging => 0xa004d1504ccd66be => 54
	i64 11705530742807338875, ; 176: he/Microsoft.Maui.Controls.resources.dll => 0xa272663128721f7b => 9
	i64 12048689113179125827, ; 177: Microsoft.Extensions.FileProviders.Physical => 0xa7358ae968287843 => 50
	i64 12058074296353848905, ; 178: Microsoft.Extensions.FileSystemGlobbing.dll => 0xa756e2afa5707e49 => 51
	i64 12145679461940342714, ; 179: System.Text.Json => 0xa88e1f1ebcb62fba => 129
	i64 12201331334810686224, ; 180: System.Runtime.Serialization.Primitives.dll => 0xa953d6341e3bd310 => 124
	i64 12259688903360554467, ; 181: GamifyMe.App.dll => 0xaa232a1a5d9aa5e3 => 95
	i64 12269460666702402136, ; 182: System.Collections.Immutable.dll => 0xaa45e178506c9258 => 97
	i64 12310909314810916455, ; 183: Microsoft.Extensions.Localization.dll => 0xaad922cbbb5a2e67 => 52
	i64 12451044538927396471, ; 184: Xamarin.AndroidX.Fragment.dll => 0xaccaff0a2955b677 => 75
	i64 12459959602091767212, ; 185: Microsoft.AspNetCore.Components.Authorization.dll => 0xaceaab3e0e65b5ac => 37
	i64 12466513435562512481, ; 186: Xamarin.AndroidX.Loader.dll => 0xad01f3eb52569061 => 80
	i64 12475113361194491050, ; 187: _Microsoft.Android.Resource.Designer.dll => 0xad2081818aba1caa => 34
	i64 12538491095302438457, ; 188: Xamarin.AndroidX.CardView.dll => 0xae01ab382ae67e39 => 68
	i64 12550732019250633519, ; 189: System.IO.Compression => 0xae2d28465e8e1b2f => 108
	i64 12681088699309157496, ; 190: it/Microsoft.Maui.Controls.resources.dll => 0xaffc46fc178aec78 => 14
	i64 12700543734426720211, ; 191: Xamarin.AndroidX.Collection => 0xb041653c70d157d3 => 69
	i64 12823819093633476069, ; 192: th/Microsoft.Maui.Controls.resources.dll => 0xb1f75b85abe525e5 => 27
	i64 12843321153144804894, ; 193: Microsoft.Extensions.Primitives => 0xb23ca48abd74d61e => 57
	i64 13003699287675270979, ; 194: Microsoft.AspNetCore.Components.WebView.Maui => 0xb4766b9b07e27743 => 41
	i64 13221551921002590604, ; 195: ca/Microsoft.Maui.Controls.resources.dll => 0xb77c636bdebe318c => 1
	i64 13222659110913276082, ; 196: ja/Microsoft.Maui.Controls.resources.dll => 0xb78052679c1178b2 => 15
	i64 13343850469010654401, ; 197: Mono.Android.Runtime.dll => 0xb92ee14d854f44c1 => 138
	i64 13381594904270902445, ; 198: he\Microsoft.Maui.Controls.resources => 0xb9b4f9aaad3e94ad => 9
	i64 13465488254036897740, ; 199: Xamarin.Kotlin.StdLib => 0xbadf06394d106fcc => 91
	i64 13467053111158216594, ; 200: uk/Microsoft.Maui.Controls.resources.dll => 0xbae49573fde79792 => 29
	i64 13540124433173649601, ; 201: vi\Microsoft.Maui.Controls.resources => 0xbbe82f6eede718c1 => 30
	i64 13545416393490209236, ; 202: id/Microsoft.Maui.Controls.resources.dll => 0xbbfafc7174bc99d4 => 13
	i64 13550417756503177631, ; 203: Microsoft.Extensions.FileProviders.Abstractions.dll => 0xbc0cc1280684799f => 47
	i64 13572454107664307259, ; 204: Xamarin.AndroidX.RecyclerView.dll => 0xbc5b0b19d99f543b => 85
	i64 13717397318615465333, ; 205: System.ComponentModel.Primitives.dll => 0xbe5dfc2ef2f87d75 => 102
	i64 13755568601956062840, ; 206: fr/Microsoft.Maui.Controls.resources.dll => 0xbee598c36b1b9678 => 8
	i64 13814445057219246765, ; 207: hr/Microsoft.Maui.Controls.resources.dll => 0xbfb6c49664b43aad => 11
	i64 13881769479078963060, ; 208: System.Console.dll => 0xc0a5f3cade5c6774 => 105
	i64 13959074834287824816, ; 209: Xamarin.AndroidX.Fragment => 0xc1b8989a7ad20fb0 => 75
	i64 14082136096249122791, ; 210: Microsoft.AspNetCore.Metadata => 0xc36dcc2b4fa28fe7 => 42
	i64 14100563506285742564, ; 211: da/Microsoft.Maui.Controls.resources.dll => 0xc3af43cd0cff89e4 => 3
	i64 14124974489674258913, ; 212: Xamarin.AndroidX.CardView => 0xc405fd76067d19e1 => 68
	i64 14125464355221830302, ; 213: System.Threading.dll => 0xc407bafdbc707a9e => 132
	i64 14461014870687870182, ; 214: System.Net.Requests.dll => 0xc8afd8683afdece6 => 116
	i64 14464374589798375073, ; 215: ru\Microsoft.Maui.Controls.resources => 0xc8bbc80dcb1e5ea1 => 24
	i64 14522721392235705434, ; 216: el/Microsoft.Maui.Controls.resources.dll => 0xc98b12295c2cf45a => 5
	i64 14551742072151931844, ; 217: System.Text.Encodings.Web.dll => 0xc9f22c50f1b8fbc4 => 128
	i64 14644679124885264456, ; 218: GamifyMe.Shared => 0xcb3c5a12c05d5848 => 93
	i64 14669215534098758659, ; 219: Microsoft.Extensions.DependencyInjection.dll => 0xcb9385ceb3993c03 => 45
	i64 14705122255218365489, ; 220: ko\Microsoft.Maui.Controls.resources => 0xcc1316c7b0fb5431 => 16
	i64 14744092281598614090, ; 221: zh-Hans\Microsoft.Maui.Controls.resources => 0xcc9d89d004439a4a => 32
	i64 14832630590065248058, ; 222: System.Security.Claims => 0xcdd816ef5d6e873a => 126
	i64 14852515768018889994, ; 223: Xamarin.AndroidX.CursorAdapter.dll => 0xce1ebc6625a76d0a => 72
	i64 14892012299694389861, ; 224: zh-Hant/Microsoft.Maui.Controls.resources.dll => 0xceab0e490a083a65 => 33
	i64 14904040806490515477, ; 225: ar\Microsoft.Maui.Controls.resources => 0xced5ca2604cb2815 => 0
	i64 14954917835170835695, ; 226: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 0xcf8a8a895a82ecef => 46
	i64 14987728460634540364, ; 227: System.IO.Compression.dll => 0xcfff1ba06622494c => 108
	i64 15024878362326791334, ; 228: System.Net.Http.Json => 0xd0831743ebf0f4a6 => 113
	i64 15076659072870671916, ; 229: System.ObjectModel.dll => 0xd13b0d8c1620662c => 118
	i64 15111608613780139878, ; 230: ms\Microsoft.Maui.Controls.resources => 0xd1b737f831192f66 => 17
	i64 15115185479366240210, ; 231: System.IO.Compression.Brotli.dll => 0xd1c3ed1c1bc467d2 => 107
	i64 15133485256822086103, ; 232: System.Linq.dll => 0xd204f0a9127dd9d7 => 111
	i64 15227001540531775957, ; 233: Microsoft.Extensions.Configuration.Abstractions.dll => 0xd3512d3999b8e9d5 => 44
	i64 15370028218478381000, ; 234: Microsoft.Extensions.Localization.Abstractions => 0xd54d4f3b162247c8 => 53
	i64 15370334346939861994, ; 235: Xamarin.AndroidX.Core.dll => 0xd54e65a72c560bea => 71
	i64 15391712275433856905, ; 236: Microsoft.Extensions.DependencyInjection.Abstractions => 0xd59a58c406411f89 => 46
	i64 15427448221306938193, ; 237: Microsoft.AspNetCore.Components.Web => 0xd6194e6b4dbb6351 => 39
	i64 15481710163200268842, ; 238: Microsoft.Extensions.FileProviders.Composite => 0xd6da155e291b5a2a => 48
	i64 15527772828719725935, ; 239: System.Console => 0xd77dbb1e38cd3d6f => 105
	i64 15536481058354060254, ; 240: de\Microsoft.Maui.Controls.resources => 0xd79cab34eec75bde => 4
	i64 15582737692548360875, ; 241: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 0xd841015ed86f6aab => 79
	i64 15592226634512578529, ; 242: Microsoft.AspNetCore.Authorization.dll => 0xd862b7834f81b7e1 => 35
	i64 15609085926864131306, ; 243: System.dll => 0xd89e9cf3334914ea => 135
	i64 15661133872274321916, ; 244: System.Xml.ReaderWriter.dll => 0xd9578647d4bfb1fc => 134
	i64 15664356999916475676, ; 245: de/Microsoft.Maui.Controls.resources.dll => 0xd962f9b2b6ecd51c => 4
	i64 15735700225633954557, ; 246: Microsoft.Extensions.Localization => 0xda606ffbe0f22afd => 52
	i64 15743187114543869802, ; 247: hu/Microsoft.Maui.Controls.resources.dll => 0xda7b09450ae4ef6a => 12
	i64 15783653065526199428, ; 248: el\Microsoft.Maui.Controls.resources => 0xdb0accd674b1c484 => 5
	i64 16154507427712707110, ; 249: System => 0xe03056ea4e39aa26 => 135
	i64 16288847719894691167, ; 250: nb\Microsoft.Maui.Controls.resources => 0xe20d9cb300c12d5f => 18
	i64 16321164108206115771, ; 251: Microsoft.Extensions.Logging.Abstractions.dll => 0xe2806c487e7b0bbb => 55
	i64 16649148416072044166, ; 252: Microsoft.Maui.Graphics => 0xe70da84600bb4e86 => 63
	i64 16677317093839702854, ; 253: Xamarin.AndroidX.Navigation.UI => 0xe771bb8960dd8b46 => 84
	i64 16890310621557459193, ; 254: System.Text.RegularExpressions.dll => 0xea66700587f088f9 => 130
	i64 16942731696432749159, ; 255: sk\Microsoft.Maui.Controls.resources => 0xeb20acb622a01a67 => 25
	i64 16998075588627545693, ; 256: Xamarin.AndroidX.Navigation.Fragment => 0xebe54bb02d623e5d => 82
	i64 17008137082415910100, ; 257: System.Collections.NonGeneric => 0xec090a90408c8cd4 => 98
	i64 17031351772568316411, ; 258: Xamarin.AndroidX.Navigation.Common.dll => 0xec5b843380a769fb => 81
	i64 17062143951396181894, ; 259: System.ComponentModel.Primitives => 0xecc8e986518c9786 => 102
	i64 17079998892748052666, ; 260: Microsoft.AspNetCore.Components.dll => 0xed08587fce5018ba => 36
	i64 17089008752050867324, ; 261: zh-Hans/Microsoft.Maui.Controls.resources.dll => 0xed285aeb25888c7c => 32
	i64 17187273293601214786, ; 262: System.ComponentModel.Annotations.dll => 0xee8575ff9aa89142 => 101
	i64 17205988430934219272, ; 263: Microsoft.Extensions.FileSystemGlobbing => 0xeec7f35113509a08 => 51
	i64 17230721278011714856, ; 264: System.Private.Xml.Linq => 0xef1fd1b5c7a72d28 => 120
	i64 17342750010158924305, ; 265: hi\Microsoft.Maui.Controls.resources => 0xf0add33f97ecc211 => 10
	i64 17438153253682247751, ; 266: sk/Microsoft.Maui.Controls.resources.dll => 0xf200c3fe308d7847 => 25
	i64 17514990004910432069, ; 267: fr\Microsoft.Maui.Controls.resources => 0xf311be9c6f341f45 => 8
	i64 17623389608345532001, ; 268: pl\Microsoft.Maui.Controls.resources => 0xf492db79dfbef661 => 20
	i64 17702523067201099846, ; 269: zh-HK/Microsoft.Maui.Controls.resources.dll => 0xf5abfef008ae1846 => 31
	i64 17704177640604968747, ; 270: Xamarin.AndroidX.Loader => 0xf5b1dfc36cac272b => 80
	i64 17710060891934109755, ; 271: Xamarin.AndroidX.Lifecycle.ViewModel => 0xf5c6c68c9e45303b => 78
	i64 17712670374920797664, ; 272: System.Runtime.InteropServices.dll => 0xf5d00bdc38bd3de0 => 122
	i64 18025913125965088385, ; 273: System.Threading => 0xfa28e87b91334681 => 132
	i64 18096026207707417514, ; 274: MudBlazor => 0xfb21fff5848f4baa => 64
	i64 18099568558057551825, ; 275: nl/Microsoft.Maui.Controls.resources.dll => 0xfb2e95b53ad977d1 => 19
	i64 18121036031235206392, ; 276: Xamarin.AndroidX.Navigation.Common => 0xfb7ada42d3d42cf8 => 81
	i64 18245806341561545090, ; 277: System.Collections.Concurrent.dll => 0xfd3620327d587182 => 96
	i64 18305135509493619199, ; 278: Xamarin.AndroidX.Navigation.Runtime.dll => 0xfe08e7c2d8c199ff => 83
	i64 18324163916253801303 ; 279: it\Microsoft.Maui.Controls.resources => 0xfe4c81ff0a56ab57 => 14
], align 16

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [280 x i32] [
	i32 57, ; 0
	i32 139, ; 1
	i32 62, ; 2
	i32 110, ; 3
	i32 69, ; 4
	i32 86, ; 5
	i32 95, ; 6
	i32 7, ; 7
	i32 49, ; 8
	i32 131, ; 9
	i32 104, ; 10
	i32 10, ; 11
	i32 74, ; 12
	i32 90, ; 13
	i32 18, ; 14
	i32 82, ; 15
	i32 109, ; 16
	i32 115, ; 17
	i32 59, ; 18
	i32 138, ; 19
	i32 131, ; 20
	i32 16, ; 21
	i32 67, ; 22
	i32 79, ; 23
	i32 112, ; 24
	i32 119, ; 25
	i32 58, ; 26
	i32 66, ; 27
	i32 6, ; 28
	i32 86, ; 29
	i32 28, ; 30
	i32 60, ; 31
	i32 28, ; 32
	i32 78, ; 33
	i32 2, ; 34
	i32 20, ; 35
	i32 74, ; 36
	i32 96, ; 37
	i32 41, ; 38
	i32 24, ; 39
	i32 77, ; 40
	i32 128, ; 41
	i32 70, ; 42
	i32 125, ; 43
	i32 65, ; 44
	i32 27, ; 45
	i32 45, ; 46
	i32 2, ; 47
	i32 58, ; 48
	i32 94, ; 49
	i32 7, ; 50
	i32 90, ; 51
	i32 35, ; 52
	i32 76, ; 53
	i32 117, ; 54
	i32 92, ; 55
	i32 47, ; 56
	i32 62, ; 57
	i32 43, ; 58
	i32 87, ; 59
	i32 136, ; 60
	i32 22, ; 61
	i32 125, ; 62
	i32 44, ; 63
	i32 134, ; 64
	i32 43, ; 65
	i32 85, ; 66
	i32 40, ; 67
	i32 42, ; 68
	i32 54, ; 69
	i32 50, ; 70
	i32 60, ; 71
	i32 116, ; 72
	i32 112, ; 73
	i32 124, ; 74
	i32 121, ; 75
	i32 33, ; 76
	i32 104, ; 77
	i32 126, ; 78
	i32 110, ; 79
	i32 103, ; 80
	i32 30, ; 81
	i32 38, ; 82
	i32 109, ; 83
	i32 0, ; 84
	i32 101, ; 85
	i32 65, ; 86
	i32 87, ; 87
	i32 123, ; 88
	i32 99, ; 89
	i32 99, ; 90
	i32 39, ; 91
	i32 123, ; 92
	i32 26, ; 93
	i32 29, ; 94
	i32 107, ; 95
	i32 127, ; 96
	i32 89, ; 97
	i32 53, ; 98
	i32 23, ; 99
	i32 37, ; 100
	i32 23, ; 101
	i32 129, ; 102
	i32 34, ; 103
	i32 77, ; 104
	i32 11, ; 105
	i32 73, ; 106
	i32 56, ; 107
	i32 19, ; 108
	i32 22, ; 109
	i32 133, ; 110
	i32 117, ; 111
	i32 40, ; 112
	i32 120, ; 113
	i32 26, ; 114
	i32 111, ; 115
	i32 103, ; 116
	i32 133, ; 117
	i32 118, ; 118
	i32 114, ; 119
	i32 17, ; 120
	i32 136, ; 121
	i32 91, ; 122
	i32 67, ; 123
	i32 64, ; 124
	i32 38, ; 125
	i32 100, ; 126
	i32 76, ; 127
	i32 113, ; 128
	i32 106, ; 129
	i32 137, ; 130
	i32 100, ; 131
	i32 63, ; 132
	i32 127, ; 133
	i32 84, ; 134
	i32 97, ; 135
	i32 119, ; 136
	i32 21, ; 137
	i32 137, ; 138
	i32 83, ; 139
	i32 21, ; 140
	i32 106, ; 141
	i32 31, ; 142
	i32 89, ; 143
	i32 66, ; 144
	i32 130, ; 145
	i32 70, ; 146
	i32 98, ; 147
	i32 139, ; 148
	i32 61, ; 149
	i32 6, ; 150
	i32 114, ; 151
	i32 121, ; 152
	i32 59, ; 153
	i32 3, ; 154
	i32 73, ; 155
	i32 94, ; 156
	i32 92, ; 157
	i32 71, ; 158
	i32 1, ; 159
	i32 93, ; 160
	i32 49, ; 161
	i32 115, ; 162
	i32 55, ; 163
	i32 61, ; 164
	i32 48, ; 165
	i32 12, ; 166
	i32 88, ; 167
	i32 36, ; 168
	i32 15, ; 169
	i32 56, ; 170
	i32 72, ; 171
	i32 122, ; 172
	i32 13, ; 173
	i32 88, ; 174
	i32 54, ; 175
	i32 9, ; 176
	i32 50, ; 177
	i32 51, ; 178
	i32 129, ; 179
	i32 124, ; 180
	i32 95, ; 181
	i32 97, ; 182
	i32 52, ; 183
	i32 75, ; 184
	i32 37, ; 185
	i32 80, ; 186
	i32 34, ; 187
	i32 68, ; 188
	i32 108, ; 189
	i32 14, ; 190
	i32 69, ; 191
	i32 27, ; 192
	i32 57, ; 193
	i32 41, ; 194
	i32 1, ; 195
	i32 15, ; 196
	i32 138, ; 197
	i32 9, ; 198
	i32 91, ; 199
	i32 29, ; 200
	i32 30, ; 201
	i32 13, ; 202
	i32 47, ; 203
	i32 85, ; 204
	i32 102, ; 205
	i32 8, ; 206
	i32 11, ; 207
	i32 105, ; 208
	i32 75, ; 209
	i32 42, ; 210
	i32 3, ; 211
	i32 68, ; 212
	i32 132, ; 213
	i32 116, ; 214
	i32 24, ; 215
	i32 5, ; 216
	i32 128, ; 217
	i32 93, ; 218
	i32 45, ; 219
	i32 16, ; 220
	i32 32, ; 221
	i32 126, ; 222
	i32 72, ; 223
	i32 33, ; 224
	i32 0, ; 225
	i32 46, ; 226
	i32 108, ; 227
	i32 113, ; 228
	i32 118, ; 229
	i32 17, ; 230
	i32 107, ; 231
	i32 111, ; 232
	i32 44, ; 233
	i32 53, ; 234
	i32 71, ; 235
	i32 46, ; 236
	i32 39, ; 237
	i32 48, ; 238
	i32 105, ; 239
	i32 4, ; 240
	i32 79, ; 241
	i32 35, ; 242
	i32 135, ; 243
	i32 134, ; 244
	i32 4, ; 245
	i32 52, ; 246
	i32 12, ; 247
	i32 5, ; 248
	i32 135, ; 249
	i32 18, ; 250
	i32 55, ; 251
	i32 63, ; 252
	i32 84, ; 253
	i32 130, ; 254
	i32 25, ; 255
	i32 82, ; 256
	i32 98, ; 257
	i32 81, ; 258
	i32 102, ; 259
	i32 36, ; 260
	i32 32, ; 261
	i32 101, ; 262
	i32 51, ; 263
	i32 120, ; 264
	i32 10, ; 265
	i32 25, ; 266
	i32 8, ; 267
	i32 20, ; 268
	i32 31, ; 269
	i32 80, ; 270
	i32 78, ; 271
	i32 122, ; 272
	i32 132, ; 273
	i32 64, ; 274
	i32 19, ; 275
	i32 81, ; 276
	i32 96, ; 277
	i32 83, ; 278
	i32 14 ; 279
], align 16

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 0, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [0 x %struct.MarshalMethodsManagedClass] zeroinitializer, align 8

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [0 x ptr] zeroinitializer, align 8

@mm_method_names = dso_local local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 0, ; id 0x0; name: 
		ptr @.MarshalMethodName.0_name; char* name
	} ; 0
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 8

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
	store ptr %fn, ptr @get_function_pointer, align 8, !tbaa !3
	ret void
}

; Strings
@.str.0 = private unnamed_addr constant [40 x i8] c"get_function_pointer MUST be specified\0A\00", align 16

;MarshalMethodName
@.MarshalMethodName.0_name = private unnamed_addr constant [1 x i8] c"\00", align 1

; External functions

; Function attributes: noreturn "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+crc32,+cx16,+cx8,+fxsr,+mmx,+popcnt,+sse,+sse2,+sse3,+sse4.1,+sse4.2,+ssse3,+x87" "tune-cpu"="generic" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { noreturn "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+crc32,+cx16,+cx8,+fxsr,+mmx,+popcnt,+sse,+sse2,+sse3,+sse4.1,+sse4.2,+ssse3,+x87" "tune-cpu"="generic" }

; Metadata
!llvm.module.flags = !{!0, !1}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!"Xamarin.Android remotes/origin/release/8.0.4xx @ 82d8938cf80f6d5fa6c28529ddfbdb753d805ab4"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
