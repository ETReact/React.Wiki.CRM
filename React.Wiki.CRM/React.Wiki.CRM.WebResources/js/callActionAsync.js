function callActionAsync(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    formContext.ui.setFormNotification("Attendere...", "INFO", "info01");

    var rct_ActionNameRequest = {
        getMetadata: function () {
            return {
                boundParameter: null,
                parameterTypes: {},
                operationType: 0,
                operationName: "rct_ActionName"
            };
        }
    };

    Xrm.WebApi.online.execute(rct_ActionNameRequest).then(
        function success(result) {
            if (result.ok) {
                formContext.ui.clearFormNotification("info01");

                result.text().then(
                    function (res) {
                        Xrm.Utility.alertDialog("SUCCESS: " + JSON.parse(res)?.output);
                    });
            }
        },
        function (error) {
            formContext.ui.clearFormNotification("info01");
            Xrm.Utility.alertDialog("ERROR: " + error.message);
        }
    );
}