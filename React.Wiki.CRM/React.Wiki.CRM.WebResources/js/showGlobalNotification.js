function showGlobalNotification() {
    var pageType = Xrm.Utility.getPageContext().input.pageType;
    var entityName = Xrm.Utility.getPageContext().input.entityName;

    var objNotification =
    {
        type: 2,
        level: 4, // 1: Success, 2: Error, 3: Warning, 4: Information
        message: "My Global Notification.",
        showCloseButton: true,
        action: null
    };

    if (pageType == "entitylist" && entityName == "[rct_myEntityName]") {
        Xrm.App.addGlobalNotification(objNotification).then(
            function success(result) {
                // add code on notification display
            },
            function (error) {
                // add code to handle error
            }
        );
    }    
}