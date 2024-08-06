function callWorkflowById(workflowId, formContext) {
    var functionName = "callWorkflowById >>";
    var query = "";
    try {

        query = "workflows(" + workflowId + ")/Microsoft.Dynamics.CRM.ExecuteWorkflow";

        var data = {
            "EntityId": formContext.data.entity.getId().replace("{", "").replace("}", "")
        };

        //Create request
        var req = new XMLHttpRequest();
        req.open("POST", formContext.context.getClientUrl() + "/api/data/v8.2/" + query, true);
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");

        req.onreadystatechange = function () {

            if (this.readyState == 4 /* complete */) {
                req.onreadystatechange = null;

                if (this.status == 200) {
                    //success callback this returns null since no return value available.
                    var result = JSON.parse(this.response);

                } else {
                    //error callback
                    var error = JSON.parse(this.response).error;
                }
            }
        };
        req.send(JSON.stringify(data));

    } catch (e) {
        throwError(functionName, e);
    }
}