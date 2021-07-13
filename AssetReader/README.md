# AssetReader使用说明

## 程序入口
**`AssetsManager`** 的 **`LoadFilesAsync`** 与 **`LoadFolderAsync`** 方法

## `Reader` 类

### **`AssetsManager`**

此类用来管理Asset文件，支持加载文件和文件夹

***

```csharp
public List<SerializedFile> AssetsFileList;
```
本List中储存了所有用户导入的AssetFile  
假设我们调用 `LoadFilesAsync` 方法时传入了 `string[0]`  
则该 `AssetsFileList` 中的元素个数为 **1个**，每个元素为 `SerializedFile` 类型  

***

### **`SerializedFile`**

此类用于储存Asset的各种信息

***

```csharp
public AssetsManager assetsManager;
```
此成员代表是哪个 `AssetsManager` 对象创建了此 `SerializedFile`  

***

```csharp
public string fullName;
public string fileName;
public string originalPath;
```
`fullName` 成员是 `SerializedFile` 实例的 `Aseet` 路径，包含内部文件名  
> src\main\resources\CAB-b126f940f479be82fda32e2b675c8b7f

`fileName` 成员是 `SerializedFile` 实例的文件名，不包含路径  
> CAB-b126f940f479be82fda32e2b675c8b7f

`originalPath` 成员是 `SerializedFile` 实例的原始路径，包含实际文件名  
> src\main\resources\char_1012_skadi2.ab

***

```csharp
public string unityVersion;
public int[] version;
public BuildType buildType;
public BuildTarget m_TargetPlatform;
```
`unityVersion` 成员是 `SerializedFile` 实例的 Unity3D 版本号，如 `2017.4.39f1`  
`version` 成员是 `unityVersion` 的拆分，包含四个整数，如 `[2017, 4, 39, 1]`  
`buildType` 成员是 Unity3D 的编译类型，如 `f`  
`m_TargetPlatform` 成员是 Unity3D 的编译目标平台，如 `StandaloneWindows`

***

```csharp
public List<UObject> Objects;
public Dictionary<long, UObject> ObjectsDic;
```
`Objects` 成员是 `SerializedFile` 实例的 `UObject` 列表，重要且常用  
`ObjectsDic` 成员中，key 对应 `UObject` 的 `m_PathID`，不常用  

***

```csharp
public List<SerializedType> m_Types;
public List<ObjectInfo> m_Objects;
```
`m_Type` 与 `m_Objects` 是为 `Objects` 服务的  
它们在 `AssetsManager` 中的 `readAssets` 方法中使用  
用于生成对应的 `UObject` 实例  

***

## **`Unity3D` 类**

### **`UObject`**

此类是所有有关于 Unity3D 的类的**基类**，包含了一些基本的属性和方法  
被 `EditorExtension` / `ResourceManager` / `PlayerSettings` / `BuildSettings` 直接继承

***

```csharp
public SerializedFile assetsFile;
```
此成员代表 `UObject` 对应的 `SerializedFile` 实例  
意为：这个对象存在于哪个Aseet文件中

***

```csharp
public long m_PathID;
```
`Unity3D` 中的全局文件索引ID，每一个 `UObject` 都有一个全局唯一的ID

***

```csharp
public ClassIDType type;
```
`type` 成员代表这个 `UObject` 在 `ClassIDType` 中对应的类型  
表明了这是一个什么类型的对象，如 `GameObject`、`MonoBehaviour`、`Texture2D` 等

***

```csharp
public uint byteSize;
```
`byteSize` 成员代表这个 `UObject` 的一般大小，单位为字节  
根据不同的 `type` 值可能不同  
如 `AudioClip` 在 `m_Source` 不为空时的大小为 `byteSize + AudioClip.m_Size`

***

#### **`EditorExtension`**

抽象类，被 `Component` / `NamedObject` / `GameObject` 直接继承  
所有有实际意义的实例对象都是它的子类

***

##### **`GameObject`**

`Unity3D` 中所有的游戏物件都是这个类型的对象，场景中所有实体的基类  
**可以导出**为 `.fbx` 文件，没有类继承它  

***

```csharp
public PPtr<Component>[] m_Components;
```
`m_Components` 成员是 `GameObject` 的组件列表，每个元素是 `Component` 的指针

***

```csharp
public string m_Name;
```
`m_Name` 成员是 `GameObject` 的名称

***

##### **`Component`**

抽象类，附加到 `GameObject` 的所有内容的基本类  
被 `Behaviour` / `MeshFilter` / `Transform` / `Renderer` 直接继承

***

```csharp
public PPtr<GameObject> m_GameObject;
```
`m_GameObject` 成员是 `Component` 对应的 `GameObject` 指针  
表示此 `Component` 在这个 `GameObject` 中

***

###### **`Behaviour`**

抽象类，是指可启用或禁用的组件  
被 `Animator` / `MonoBehaviour` / `Animation` 直接继承

***

###### **`Renderer`**
抽象类，所有渲染器的常规功能  
渲染器是使对象显示在屏幕上的工具  
使用该类可以访问任何对象、网格或粒子系统的渲染器  
被 `MeshRenderer` / `SkinnedMeshRenderer` 直接继承

***

##### **`NamedObject`**

命名的对象，表示这个对象有名字  
一般从 `AssetBundle` 中获取的有用对象都是 `NamedObject` 的子类

***

```csharp
public string m_Name;
```
`m_Name` 是这个对象的名字

***

## **`Unity3D` 实体**

### **`Animator`**

用于控制 `Mecanim` 动画系统的接口  
**可以导出**为 `.fbx` 文件，对象名称为对应 `GameObject` 的名称  

***

### **`MonoBehaviour`**

是一个基类，所有 `Unity3D` 脚本都派生自该类  
**可以导出**为 `.json` 文件  
对象名称在 `m_Name` 为空，且 `m_Script` 不为空时为 `m_Script.m_ClassName`  

***

```csharp
public PPtr<MonoScript> m_Script;
```
指向 `MonoScript` 的指针，表示这个 `MonoBehaviour` 的脚本

***

### **`Animation`**

动画组件用于播放动画  
本身不能导出，但是其成员 `AnimationClip` 可以导出为 `.fbx` 文件

```csharp
public PPtr<AnimationClip>[] m_Animations;
```

***

### **`MeshFilter`**

用于访问mesh filter的 Mesh 的类  
这与程序化网格接口一起使用  
本身不能导出，但是其成员 `Mesh` 可以导出为 `.obj` 文件  

```csharp
public PPtr<Mesh> m_Mesh;
```

***

### **`Transform`**

对象的位置、旋转和缩放，不能导出  
场景中的每个对象都有一个变换  
它用于存储和操作对象的位置、旋转和缩放  
每个变换都可以有一个父级  

***

```csharp
public Quaternion m_LocalRotation;
public Vector3 m_LocalPosition;
public Vector3 m_LocalScale;
public PPtr<Transform>[] m_Children;
public PPtr<Transform> m_Father;
```
`m_LocalRotation` 相对于父级变换旋转的变换旋转  
`m_LocalPosition` 相对于父变换的变换位置  
`m_LocalScale` 相对于父对象的变换缩放  
`m_Children` 此变换的子变换集合  
`m_Father` 此变换的父级变换  

***

### **`MeshRenderer`**
渲染由 MeshFilter 或 TextMesh 插入的网格  
不可导出，在 `UnityABStudio` 中无意义，本质是 `Renderer`  

***

### **`SkinnedMeshRenderer`**

蒙皮网格过滤器  
本身不能导出，但是其成员 `Mesh` 可以导出为 `.obj` 文件

```csharp
public PPtr<Mesh> m_Mesh;
```

***

### **`Material`**

材质类  
本身不能导出，但是其成员 `Shader` 可以导出为 `.shader` 文件  

```csharp
public PPtr<Shader> m_Shader;
```

***

### **`Sprite`**

表示在 2D 游戏中使用的精灵对象  
精灵 是一种 2D 图形对象，用于 2D 游戏元素  
图形是从位图图像 `Texture2D` 获取的  
`Sprite` 类主要标识应该用于特定精灵的图像部分  
`GameObject` 上的 `SpriteRenderer` 组件可以使用该信息来实际显示图形  
**可以导出**为 `Jpeg`、`Png`、`Bmp` 或 `Tga` 图像文件  

***

### **`Avatar`**

表示一个 `Mecanim` 化身，用于游戏中的角色  
它可以是一个通用化身，也可以是一个人形化身  
不可导出，在 `UnityABStudio` 中无意义  

***

### **`VideoClip`**

一种视频数据的容器  
在 `m_OriginalPath` 不为空时**可以导出**  
导出格式为 `m_OriginalPath` 中的扩展名  
数据的大小为 `byteSize + m_ExternalResources.m_Size;`  

***

### **`MovieTexture`**

已弃用，电影纹理可用于播放电影序列的动画，或将电影渲染到场景中  
**可以导出**为 `.ogv` 文件

***

### **`Texture2D`**

用于纹理处理的类  
当 `m_OriginalPath` 不为空时  
数据的大小为 `byteSize + m_StreamData.size;`  
**可以导出**为 `Jpeg`、`Png`、`Bmp`、`Tga` 或 `.tex` 文件

***

### **`AssetBundle`**
`AssetBundle` 即是 `.ab` 资源文件  
不可导出，其中 `m_Container` 会向 `AssetItem` 中的 `Container` 赋值   

```csharp
public KeyValuePair<string, AssetInfo>[] m_Container;
```

***

### **`MonoScript`**

该类表示存储在项目中的 C#、JavaScript 和 Boo 文件  
在对应的 `MonoBehaviour` 中导出  

***

### **`Font`**

font assets 的脚本接口  
可使用此类动态切换 GUI 文本或文本网格上的字体  
**可以导出**为 `.ttf` 或 `.otf` 文件  

***

### **`RuntimeAnimatorController`**

`AnimatorController` 的运行时表示  
使用此表示可在运行时期间更改 `Animator Controller`  
不可导出，在 `UnityABStudio` 中没有意义  

***

### **`TextAsset`**

文本文件资源，可以将项目中的原始文本文件用作资源，通过此类获取其内容  
**可以导出**为 `.txt` 文件  

***

### **`Mesh`**

用于通过脚本创建或修改网格的类  
**可以导出**为 `.obj` 文件  

***

### **`AudioClip`**

存储压缩为 `Ogg Vorbis` 或未压缩的音频文件  
当 `m_Source` 不为空时  
数据的大小为 `byteSize + m_Size;`  
**可以导出**为 `.m4a` `.aif` `.it` `.mod` `.mp3` `.ogg` `.s3m` `.wav` `.xm` `.wav` `.vag` `.fsb` 文件

***

### **`Shader`**

用于所有渲染的着色器脚本  
对象名称为 `m_ParsedForm?.m_Name ?? m_Name;`  
**可以导出**为 `.shader` 文件  

***

### **`AnimationClip`**

保存基于关键帧的动画  
**可以导出**为 `.fbx` 文件  

***

### **`AnimatorController`**

通过具有状态机（由参数控制）的层来控制的动画控制器  
本身不能导出，但是成员 `AnimationClip` 可以导出  

```csharp
public PPtr<AnimationClip>[] m_AnimationClips;
```

***

### **`AnimatorOverrideController`**

动画器重写控制器的用途是重写某个控制器的动画剪辑，从而为给定化身定制动画  
不能导出  

***

### **`SpriteAtlas`**

精灵图集是在 Unity 中创建的一种资源  
它是内置精灵打包解决方案的一部分  
本身不能导出，但是成员 `Sprite` 可以导出

```csharp
public PPtr<Sprite>[] m_PackedSprites;
```

***

