var MyPlugin = {
    Hello: function()
    {
        window.alert("Hello, world!");
    },  
    StringReturnValueFunction: function()
    {
        var returnStr = window.location.host;
        var buffer = _malloc(lengthBytesUTF8(returnStr) + 1);
        writeStringToMemory(returnStr, buffer);
        return buffer;
    } 
};
 
mergeInto(LibraryManager.library, MyPlugin);
