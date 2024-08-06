//pulsante visibile solo se count > 0
async function enable_Rule(primaryControl) {
    debugger;
    var formContext = primaryControl;
    var entityInformations = await getEntityInfo(formContext); //>>>>>>>>>>>>>>>>>> Promise
    
    Promise.all([entityInformations]).then((results) => { entityInformations = results; });
    if (entityInformations.entities.length > 0)
        return true;
    else
        return false;
}

function getEntityInfo(formContext) {
    return new Promise((resolve) => {
        Xrm.WebApi.online.retrieveMultipleRecords("...").then(
            function success(results) {
                resolve(results);
            },
            function (error) {
                Xrm.Utility.alertDialog(error.message);
                resolve(null);
            }
        );
    })
}