# AssetReader使用说明

## 程序入口
**`AssetsManager`** 的 **`LoadFilesAsync`** 与 **`LoadFolderAsync`** 方法

## 类成员结构

***

### **`AssetsManager`**

***

```csharp
public List<SerializedFile> AssetsFileList;
```
本List中储存了所有用户导入的AssetFile  
假设我们调用 `LoadFilesAsync` 方法时传入了 `string[0]`  
则该 `AssetsFileList` 中的元素个数为 **1个**，每个元素为 `SerializedFile` 类型  

***

### **`SerializedFile`**

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
```
`unityVersion` 成员是 `SerializedFile` 实例的 Unity3D 版本号，如 `2017.4.39f1`  
`version` 成员是 `unityVersion` 的拆分，包含四个整数，如 `[2017, 4, 39, 1]`  
`buildType` 成员是 Unity3D 的编译类型，如 `f`  

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

### **Unity3D 类**

***

```csharp

```
