angular.module("umbraco").controller("PackageExportDashboardController", function ($http, notificationsService) {
    var vm = this;
    vm.loading = false;
    vm.message = "";
    vm.logs = "";
    vm.downloadUrl = "";

    vm.runExport = function () {
        vm.loading = true;
        vm.message = "";
        vm.logs = "";
        vm.downloadUrl = "";

        $http.post("/umbraco/backoffice/api/package-export/run")
            .then(function (response) {
                vm.message = response.data.message || "打包完成";
                vm.logs = response.data.stdout || "";
                vm.downloadUrl = response.data.downloadUrl || "";
                notificationsService.success("完成", vm.message);
                if (vm.downloadUrl) {
                    window.location.href = vm.downloadUrl;
                }
            })
            .catch(function (error) {
                var data = error && error.data ? error.data : {};
                vm.message = data.message || "打包失敗";
                vm.logs = [data.stdout, data.stderr].filter(Boolean).join("\n\n");
                notificationsService.error("失敗", vm.message);
            })
            .finally(function () {
                vm.loading = false;
            });
    };
});
