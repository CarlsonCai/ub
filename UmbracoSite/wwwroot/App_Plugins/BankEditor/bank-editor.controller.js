angular.module("umbraco").controller("BankEditorController", function ($http, $sce, notificationsService) {
    var vm = this;

    vm.tab = "announcements";
    vm.page = "list"; // list | edit | export
    vm.busy = false;
    vm.banner = null;
    vm.search = { announcements: "", articles: "", products: "", promotions: "" };
    vm.sort = {
        announcements: { key: "date", dir: "desc" },
        articles: { key: "date", dir: "desc" },
        products: { key: "sort", dir: "asc" },
        promotions: { key: "title", dir: "asc" }
    };
    vm.selection = { announcements: {}, articles: {}, products: {}, promotions: {} }; // id -> true

    vm.data = { siteSettings: {}, home: {}, collections: { announcements: [], articles: [], products: [], promotions: [] } };
    vm.selectedType = "";
    vm.selectedItem = null;

    vm.trustHtml = function (html) {
        return $sce.trustAsHtml(html || "");
    };

    function showBanner(type, text) {
        vm.banner = { type: type, text: text };
    }

    function normalizeDate(value) {
        if (!value) return "";
        if (typeof value === "string") return value;
        return "";
    }

    vm.loadAll = function () {
        vm.busy = true;
        vm.banner = null;
        return $http.get("/umbraco/backoffice/api/bank-content/get")
            .then(function (response) {
                vm.data = response.data || vm.data;
                vm.data.collections = vm.data.collections || {};
                vm.data.collections.announcements = vm.data.collections.announcements || [];
                vm.data.collections.articles = vm.data.collections.articles || [];
                vm.data.collections.products = vm.data.collections.products || [];
                vm.data.collections.promotions = vm.data.collections.promotions || [];

                vm.data.collections.announcements.forEach(function (x) { x.date = normalizeDate(x.date); });
                vm.data.collections.articles.forEach(function (x) { x.date = normalizeDate(x.date); });

                // rebind selection by id if possible
                if (vm.selectedItem && vm.selectedItem.id) {
                    var list = vm.getList(vm.selectedType);
                    var found = list.find(function (x) { return x && x.id === vm.selectedItem.id; });
                    if (found) vm.selectedItem = found;
                }
            })
            .catch(function () {
                showBanner("error", "載入失敗，請稍後再試。");
            })
            .finally(function () {
                vm.busy = false;
            });
    };

    vm.save = function () {
        vm.busy = true;
        vm.banner = null;
        return $http.post("/umbraco/backoffice/api/bank-content/save", vm.data)
            .then(function (response) {
                notificationsService.success("完成", (response.data && response.data.message) || "已儲存");
                showBanner("success", "已儲存（資料已更新，可直接匯出）");
                return vm.loadAll();
            })
            .catch(function () {
                showBanner("error", "儲存失敗，請檢查欄位格式（日期建議 YYYY-MM-DD）。");
                notificationsService.error("失敗", "儲存失敗");
            })
            .finally(function () {
                vm.busy = false;
            });
    };

    vm.exportJson = function () {
        vm.busy = true;
        vm.banner = null;
        return $http.post("/umbraco/backoffice/api/package-export/export-js")
            .then(function (response) {
                var url = response.data && response.data.downloadUrl;
                notificationsService.success("完成", (response.data && response.data.message) || "JS 匯出完成");
                showBanner("success", "JS 匯出完成（會自動下載）");
                if (url) window.location.href = url;
            })
            .catch(function () {
                showBanner("error", "JS 匯出失敗。");
                notificationsService.error("失敗", "JS 匯出失敗");
            })
            .finally(function () {
                vm.busy = false;
            });
    };

    vm.exportZip = function () {
        vm.busy = true;
        vm.banner = null;
        return $http.post("/umbraco/backoffice/api/package-export/run")
            .then(function (response) {
                var url = response.data && response.data.downloadUrl;
                notificationsService.success("完成", (response.data && response.data.message) || "匯出完成");
                showBanner("success", "ZIP 匯出完成（會自動下載）");
                if (url) window.location.href = url;
            })
            .catch(function (error) {
                var data = error && error.data ? error.data : {};
                showBanner("error", data.message || "ZIP 匯出失敗。");
                notificationsService.error("失敗", data.message || "ZIP 匯出失敗");
            })
            .finally(function () {
                vm.busy = false;
            });
    };

    vm.select = function (type, item) {
        vm.selectedType = type;
        vm.selectedItem = item;
        vm.page = "edit";
    };

    vm.getList = function (type) {
        var c = (vm.data && vm.data.collections) || {};
        if (type === "announcement" || type === "announcements") return c.announcements || [];
        if (type === "article" || type === "articles") return c.articles || [];
        if (type === "product" || type === "products") return c.products || [];
        if (type === "promotion" || type === "promotions") return c.promotions || [];
        return [];
    };

    vm.addAnnouncement = function () {
        vm.data.collections = vm.data.collections || {};
        vm.data.collections.announcements = vm.data.collections.announcements || [];
        var item = {
            id: "00000000-0000-0000-0000-000000000000",
            title: "",
            summary: "",
            bodyHtml: "",
            slug: "",
            level: "important",
            date: new Date().toISOString().slice(0, 10),
            isPinned: false,
            isPublished: false
        };
        vm.data.collections.announcements.unshift(item);
        vm.select("announcement", item);
    };

    vm.addArticle = function () {
        vm.data.collections = vm.data.collections || {};
        vm.data.collections.articles = vm.data.collections.articles || [];
        var item = {
            id: "00000000-0000-0000-0000-000000000000",
            title: "",
            summary: "",
            bodyHtml: "",
            slug: "",
            category: "",
            date: new Date().toISOString().slice(0, 10),
            isPublished: false
        };
        vm.data.collections.articles.unshift(item);
        vm.select("article", item);
    };

    vm.addProduct = function () {
        vm.data.collections = vm.data.collections || {};
        vm.data.collections.products = vm.data.collections.products || [];
        var item = {
            id: "00000000-0000-0000-0000-000000000000",
            title: "",
            summary: "",
            bodyHtml: "",
            slug: "",
            sort: 10,
            isPublished: false
        };
        vm.data.collections.products.unshift(item);
        vm.select("product", item);
    };

    vm.addPromotion = function () {
        vm.data.collections = vm.data.collections || {};
        vm.data.collections.promotions = vm.data.collections.promotions || [];
        var item = {
            id: "00000000-0000-0000-0000-000000000000",
            title: "",
            summary: "",
            bodyHtml: "",
            slug: "",
            isPinned: false,
            isPublished: false
        };
        vm.data.collections.promotions.unshift(item);
        vm.select("promotion", item);
    };

    vm.goList = function () {
        vm.page = "list";
        vm.selectedType = "";
        vm.selectedItem = null;
    };

    vm.goExport = function () {
        vm.page = "export";
        vm.selectedType = "";
        vm.selectedItem = null;
    };

    vm.stats = function () {
        var a = (vm.data.collections && vm.data.collections.announcements) || [];
        var b = (vm.data.collections && vm.data.collections.articles) || [];
        var p = (vm.data.collections && vm.data.collections.products) || [];
        var m = (vm.data.collections && vm.data.collections.promotions) || [];
        return {
            announcementsTotal: a.length,
            announcementsPublished: a.filter(function (x) { return x && x.isPublished; }).length,
            articlesTotal: b.length,
            articlesPublished: b.filter(function (x) { return x && x.isPublished; }).length,
            productsTotal: p.length,
            productsPublished: p.filter(function (x) { return x && x.isPublished; }).length,
            promotionsTotal: m.length,
            promotionsPublished: m.filter(function (x) { return x && x.isPublished; }).length,
            updatedAt: vm.data.updatedAt || vm.data.UpdatedAt || ""
        };
    };

    vm.removeSelected = function () {
        if (!vm.selectedItem) return;

        var ok = window.confirm("確定要刪除？刪除後無法復原。");
        if (!ok) return;

        var list = vm.getList(vm.selectedType);
        var next = list.filter(function (x) { return x !== vm.selectedItem; });
        if (vm.selectedType === "announcement") vm.data.collections.announcements = next;
        else if (vm.selectedType === "article") vm.data.collections.articles = next;
        else if (vm.selectedType === "product") vm.data.collections.products = next;
        else if (vm.selectedType === "promotion") vm.data.collections.promotions = next;

        vm.selectedItem = null;
        vm.selectedType = "";
        vm.page = "list";
        showBanner("success", "已刪除，正在儲存…");
        vm.save();
    };

    function matchesSearch(item, q) {
        if (!q) return true;
        q = (q || "").toLowerCase();
        return [item.title, item.summary, item.slug].filter(Boolean).join(" ").toLowerCase().indexOf(q) !== -1;
    }

    function compare(a, b) {
        if (a === b) return 0;
        if (a === null || a === undefined) return -1;
        if (b === null || b === undefined) return 1;
        if (typeof a === "number" && typeof b === "number") return a - b;
        return ("" + a).localeCompare("" + b);
    }

    function sortList(list, sortState) {
        var key = sortState && sortState.key;
        var dir = sortState && sortState.dir === "asc" ? 1 : -1;
        if (!key) return list;
        return list.slice().sort(function (x, y) {
            return compare(x && x[key], y && y[key]) * dir;
        });
    }

    vm.setSort = function (type, key) {
        var s = vm.sort[type] || vm.sort.announcements;
        if (s.key === key) {
            s.dir = s.dir === "asc" ? "desc" : "asc";
        } else {
            s.key = key;
            s.dir = "asc";
        }
    };

    vm.isSelected = function (type, item) {
        var map = vm.selection[type] || {};
        return !!(item && item.id && map[item.id]);
    };

    vm.toggleSelected = function (type, item, checked) {
        var map = vm.selection[type] || (vm.selection[type] = {});
        if (!item || !item.id) return;
        if (checked) map[item.id] = true;
        else delete map[item.id];
    };

    vm.clearSelection = function (type) {
        vm.selection[type] = {};
    };

    vm.toggleSelectAll = function (type, checked) {
        var list =
            type === "announcements" ? vm.filteredAnnouncementsRaw() :
            type === "articles" ? vm.filteredArticlesRaw() :
            type === "products" ? vm.filteredProductsRaw() :
            vm.filteredPromotionsRaw();
        vm.clearSelection(type);
        if (!checked) return;
        var map = vm.selection[type] || (vm.selection[type] = {});
        list.forEach(function (x) {
            if (x && x.id) map[x.id] = true;
        });
    };

    vm.selectedCount = function (type) {
        var map = vm.selection[type] || {};
        return Object.keys(map).length;
    };

    vm.applyBulk = function (type, action) {
        var map = vm.selection[type] || {};
        var list =
            type === "announcements" ? ((vm.data.collections && vm.data.collections.announcements) || []) :
            type === "articles" ? ((vm.data.collections && vm.data.collections.articles) || []) :
            type === "products" ? ((vm.data.collections && vm.data.collections.products) || []) :
            ((vm.data.collections && vm.data.collections.promotions) || []);
        var ids = Object.keys(map);
        if (!ids.length) return;

        if (action === "delete") {
            var ok = window.confirm("確定要刪除選取的項目？刪除後無法復原。");
            if (!ok) return;
        }

        list.forEach(function (x) {
            if (!x || !x.id || !map[x.id]) return;
            if (action === "publish") x.isPublished = true;
            else if (action === "unpublish") x.isPublished = false;
        });

        if (action === "delete") {
            if (type === "announcements") {
                vm.data.collections.announcements = list.filter(function (x) { return !(x && x.id && map[x.id]); });
            } else if (type === "articles") {
                vm.data.collections.articles = list.filter(function (x) { return !(x && x.id && map[x.id]); });
            } else if (type === "products") {
                vm.data.collections.products = list.filter(function (x) { return !(x && x.id && map[x.id]); });
            } else {
                vm.data.collections.promotions = list.filter(function (x) { return !(x && x.id && map[x.id]); });
            }
        }

        vm.clearSelection(type);
        showBanner("success", "已套用批次操作，正在儲存…");
        vm.save();
    };

    vm.filteredAnnouncementsRaw = function () {
        var list = (vm.data.collections && vm.data.collections.announcements) || [];
        var q = vm.search.announcements || "";
        return list.filter(function (x) { return matchesSearch(x, q); });
    };

    vm.filteredAnnouncements = function () {
        return sortList(vm.filteredAnnouncementsRaw(), vm.sort.announcements);
    };

    vm.filteredArticlesRaw = function () {
        var list = (vm.data.collections && vm.data.collections.articles) || [];
        var q = vm.search.articles || "";
        return list.filter(function (x) { return matchesSearch(x, q); });
    };

    vm.filteredProductsRaw = function () {
        var list = (vm.data.collections && vm.data.collections.products) || [];
        var q = vm.search.products || "";
        return list.filter(function (x) { return matchesSearch(x, q); });
    };

    vm.filteredPromotionsRaw = function () {
        var list = (vm.data.collections && vm.data.collections.promotions) || [];
        var q = vm.search.promotions || "";
        return list.filter(function (x) { return matchesSearch(x, q); });
    };

    vm.filteredArticles = function () {
        return sortList(vm.filteredArticlesRaw(), vm.sort.articles);
    };

    vm.loadAll();
});

