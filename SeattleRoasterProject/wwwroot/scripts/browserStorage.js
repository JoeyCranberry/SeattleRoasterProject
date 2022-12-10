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
    var storedObject = JSON.parse(GetValueFromStorage(key));
    var index = -1;
    
    // Search object for value
    for (i = 0; i < storedObject.length; ++i)
    {
        if (storedObject[i].Id === value.Id) {
            index = i;
        }
    }

    if (index > -1) {
        storedObject.splice(index, 1);

        SaveObjectToStorage(key, storedObject);
    }
}

function AddValueToList(key, value) {
    var storedObject = JSON.parse(GetValueFromStorage(key));
    if (storedObject === null)
    {
        storedObject = [];
    }

    storedObject.push(value);
    SaveObjectToStorage(key, storedObject);
}
