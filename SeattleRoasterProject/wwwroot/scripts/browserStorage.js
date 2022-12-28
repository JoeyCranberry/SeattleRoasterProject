function SaveObjectToStorage(key, object) {
    localStorage.setItem(key, JSON.stringify(object));
}

function RemoveObjectFromStorage(key) {
    if (GetValueFromStorage(key) !== null) {
        localStorage.removeItem(key);
    }
}

function GetValueFromStorage(key) {
    return localStorage.getItem(key);
}

function RemoveValueFromList(key, value) {
    var rawObject = GetValueFromStorage(key); 

    if (rawObject === null || rawObject === "") {
        return;
    }

    var parsedObject = JSON.parse(rawObject);  
    var index = -1;
    
    // Search object for value
    for (i = 0; i < parsedObject.length; ++i)
    {
        if (parsedObject[i].Id === value.Id) {
            index = i;
        }
    }

    if (index > -1) {
        parsedObject.splice(index, 1);

        SaveObjectToStorage(key, parsedObject);
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
