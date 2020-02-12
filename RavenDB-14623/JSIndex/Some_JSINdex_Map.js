map('TestDocuments', function (doc) {
    var res = [];

    res.push({
        NewPropertyName: doc.ArrayOfStrings,
        NameButDifferentName: doc.Name
    });

    return res;
});