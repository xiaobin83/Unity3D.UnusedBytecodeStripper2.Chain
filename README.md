# Unity3D.UnusedBytecodeStripper2.Chain

Unity3D UnusedBytecodeStripper2 wrapper. For processing built dlls before il2cpp.

When unity building iOS player, UnusedBytecodeStripper2 will be used to strip unused code from dlls needs to be converted to C++ code.
This tool wraps UnusedBytecodeStripper2, make it execute IProcessDll defined in dlls at the same location of UnusedBytecodeStripper2.exe 
before executing original UnusedBytecodeStripiper2.

The idea came from [Unity3D.UselessAttributeStripper](https://github.com/SaladLab/Unity3D.UselessAttributeStripper).

Check [modified Unity3D.UselessAttributeStripper](https://github.com/xiaobin83/Unity3D.UselessAttributeStripper) and [Unity3D.HotPatchEnabler](https://github.com/xiaobin83/Unity3D.HotPatchEnabler) that work with it.

