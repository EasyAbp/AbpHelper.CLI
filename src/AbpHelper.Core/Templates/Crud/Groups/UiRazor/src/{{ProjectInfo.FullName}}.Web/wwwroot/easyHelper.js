/* 
   This code is generated from AbpHelper.Cli. 
   If the code is regenerated, the manual changes to this file will be overwritten.
   CreationTime:2023/12/18
   LastModificationTime:2023/12/18
*/

var easyHelper = easyHelper || {};
(function () {
    var localizer = abp.localization.getResource('{{ ProjectInfo.Name }}');

    easyHelper.setFilterToggle = function (formId) {
        let column = 12 / $('#' + formId + ' >div >div')[0].classList[1].split('-')[2]
        function FilterToggleEvent(e) {
            var isClosed = e.currentTarget.classList.toggle('fa-angle-double-down');
            e.currentTarget.classList.toggle('fa-angle-double-up');
            e.currentTarget.innerText = localizer(isClosed ? 'Expand' : 'Collapse');
            var element = document.querySelector("#" + formId + " > div > div:nth-child(" + column + ")");

            while (element.nextElementSibling) {
                element = element.nextElementSibling;
                $(element).slideToggle();
            }
            
            var customerElement = document.querySelector("#" + formId + " > div");
            customerElement.nextElementSibling

            while (customerElement.nextElementSibling) {
                customerElement = customerElement.nextElementSibling;
                $(customerElement).slideToggle();
            }
        }
        var filterItems = $('#' + formId + ' > div > div');
        var collapse_Btn = $('#' + formId.replace('Filter', '') + 'Collapse');

        filterItems.length <= column ?
            collapse_Btn.hide() :
            collapse_Btn.click(FilterToggleEvent).trigger('click');
    }

    easyHelper.serializeForm = function (formId) {
        var input = {};
        var pattern = formId + `\\.`;
        $("#" + formId).serializeArray().forEach(function (data) {
            var value = data.value || null;
            var keys = data.name.replace(new RegExp(pattern, "g"), '').split('.').map(key => abp.utils.toCamelCase(key));
            var lastKey = keys.pop();
            if (keys.length == 0 && !value) return;
            var lastObj = keys.reduce((obj, key) => obj[key] = obj[key] || {}, input);
            lastObj[lastKey] = value;
        })
        return input;
    };
})();
