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
所有有实际意义的游戏对象都是它的子类

***

##### **`GameObject`**

`Unity3D` 中所有的游戏物件都是这个类型的对象，场景中所有实体的基类  
在 `UnityABStudio` 中此对象会导出为 `.fbx` 文件，没有类继承它  

***

##### **`Component`**

抽象类，附加到 `GameObject` 的所有内容的基本类  
被 `Behaviour` / `MeshFilter` / `Transform` / `Renderer` 直接继承

***

###### **`Behaviour`**

Behaviour 是指可启用或禁用的组件  
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
`m_Name` 这个对象的名字

***

## **`Unity3D` 实体**

### **`Animator`**

