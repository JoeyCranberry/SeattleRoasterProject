function SaveObjectToStorage(key, object) {
    localStorage.setItem(key, JSON.stringify(object));
}

function RemoveObjectFromStorage(key) {
    if (GetValueFromStorage(key) !== null) {
        localStorage.removeItem(key);
    } else {
        console.wann("Could not find object with key: (" + key + ")")
    }
}

function GetValueFromStorage(key) {
    return localStorage.getItem(key);
}

function RemoveValueFromList(key, valueId) {
    var rawObject = GetValueFromStorage(key); 

    if (rawObject === null || rawObject === "") {
        console.warn("Could not find object matching key: " + key);
        return;
    }

    var parsedObject = JSON.parse(rawObject);  
    var index = -1;
    
    // Search object for value
    for (i = 0; i < parsedObject.length; ++i)
    {
        if (parsedObject[i].id === valueId && parsedObject[i].id != undefined) {
            index = i;
        }
    }

    if (index > -1) {
        parsedObject.splice(index, 1);

        SaveObjectToStorage(key, parsedObject);
    } else {
        console.warn("Could not find value in " + key + " with matching id: " + valueId);
    }
}

function AddValueToList(key, value) {
    var rawObject = GetValueFromStorage(key); 
    var storedObject;
    if (rawObject === null) {
        storedObject = [];
    }
    else
    {
        storedObject = JSON.parse(rawObject);
    }
    

    storedObject.push(value);
    SaveObjectToStorage(key, storedObject);
}
