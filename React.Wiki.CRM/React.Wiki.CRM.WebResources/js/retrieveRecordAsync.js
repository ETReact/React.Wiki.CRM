//HTTP REQUEST USING FETCH XML
function retrieveHTTPfetchXML(entityCollection, fetchXml) {
    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/" + entityCollection /* i.e. accounts */ + "?fetchXml=" + encodeURIComponent(fetchXml), true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                for (var i = 0; i < results.value.length; i++) {
                    console.log("Retrieved values: " + results.value[i].rct_fieldname);
                }

            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();
}

//HTTP REQUEST USING ODATA
function retrieveHTTPfilterOData(entityCollection, value) {
    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/" + entityCollection /* i.e. accounts */ + "?$select=rct_fieldname&$filter=rct_fieldname eq " + value, true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                for (var i = 0; i < results.value.length; i++) {
                    console.log("Retrieved values: " + results.value[i].rct_fieldname);
                }

            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();
}

//XRM WEB API FOR SINGLE RECORD
function retrieveRecordXRMAPI(entityName, id) {
    Xrm.WebApi.online.retrieveRecord(entityName /* i.e. account */, id.replace("{", "").replace("}", ""), "?$select=rct_fieldname").then(
        function success(result) {
            console.log("Retrieved value: " + result.rct_fieldname);
        },
        function (error) {
            Xrm.Utility.alertDialog(error.message);
        }
    );
}

//XRM WEB API FOR MULTIPLE RECORDS
function retrieveMultipleRecordsXRMAPI(entityName, value) {
    Xrm.WebApi.online.retrieveMultipleRecords(entityName /* i.e. account */, "?$select=rct_fieldname&$filter=rct_fieldname eq " + value).then(
        function success(results) {
            for (var i = 0; i < results.entities.length; i++) {
                console.log("Retrieved values: " + results.entities[i].rct_fieldname);
            }
        },
        function (error) {
            Xrm.Utility.alertDialog(error.message);
        }
    );
}
