/* 
   This code is generated from AbpHelper.Cli. 
   If the code is regenerated, the manual changes to this file will be overwritten.
   CreationTime:2023/12/18
   LastModificationTime:2024/12/28
*/

var easyHelper = easyHelper || {};
(function () {
    var localizer = abp.localization.getResource('SManagement');

    easyHelper.setFilterToggle = function (formId) {
        let column = 12 / $(formId + ' >div >div')[0].classList[1].split('-')[2]
        function FilterToggleEvent(e) {
            var isClosed = e.currentTarget.classList.toggle('fa-angle-double-down');
            e.currentTarget.classList.toggle('fa-angle-double-up');
            e.currentTarget.innerText = localizer(isClosed ? 'Expand' : 'Collapse');
            var dynamicElement = document.querySelector(formId + " > div > div:nth-child(" + column + ")");

            while (dynamicElement.nextElementSibling) {
                dynamicElement = dynamicElement.nextElementSibling;
                $(dynamicElement).slideToggle();
            }

            var customerElement = document.querySelector(formId + " > div");
            customerElement.nextElementSibling

            while (customerElement.nextElementSibling) {
                customerElement = customerElement.nextElementSibling;
                $(customerElement).slideToggle();
            }
        }
        var filterItems = $(formId + ' > div > div');
        var collapse_Btn = $(formId.replace('Filter', '') + 'Collapse');

        filterItems.length <= column ?
            collapse_Btn.hide() :
            collapse_Btn.click(FilterToggleEvent).trigger('click');
    }

    /**
     * 
     * @param {string} formId - the form id
     * - etc. '#myForm'
     * @returns {object} - the form data
     */
    easyHelper.serializeForm = function (formId) {
        return () => {
            var input = {};
            var pattern = formId + `\\.`;
            $(formId).serializeArray().forEach(function (data) {
                var value = data.value || null;
                var keys = data.name.replace(new RegExp(pattern, "g"), '').split('.').map(key => abp.utils.toCamelCase(key));
                var lastKey = keys.pop();
                if (keys.length == 0 && !value) return;
                var lastObj = keys.reduce((obj, key) => obj[key] = obj[key] || {}, input);
                lastObj[lastKey] = value;
            })
            return input;
        }
    };

    /** 
     * @function - easy binding select2
     * @param {string} selector - the select2 selector
     * - etc. '#Id'
     * @param {function} apiGetList - the api function to get the list
     * 
     * - etc. (input) =>{ return api.getList(input) }
     * 
     * - return type: [{ id: item.id, text: item.text || item.displayName || item.name }]
     * @param {object} options - if the select2 is in the modal
     * - { isModal: true,  ...other select2 options}
     * @param {function} apiGet - the api function to get the single
     * @returns { id: item.id, text: item.text || item.displayName || item.name } -the select2 data
     * @example
     * easyHelper.select2('#BookName',
     * (input) => {
     *         input.type = 1;
     *         input.name = input.term; //the select input value
     *         return acme.bookStore.books.book.getList(input)
     * },
     * {isModal:true},
     * acme.bookStore.books.book.get) //set the default value if has value
     * 
    **/
    easyHelper.select2 = (selector, apiGetList, options, apiGet) => {
        var defaultOptions = {
            placeholder: ' ',
            allowClear: true,
            ajax: {
                transport: function (params, success, failure) {
                    const param = {
                        ...params.data,
                        skipCount: 0,
                        maxResultCount: 10,
                        sorting: null
                    };
                    apiGetList(param).then(success).catch(failure);
                },
                processResults: function (data, params) {
                    return {
                        results: data.items.map(item => {
                            return {
                                id: item.id,
                                text: item.text || item.displayName || item.name
                            };
                        }),
                        pagination: {
                            more: (params.page * 10) < data.totalCount
                        }
                    };
                }
            }
        }

        if (selector[0] != '#') return;

        var element = document.getElementById(selector.replace('#', ''));

        if (!element) return;

        var value = element.attributes['value'].value;
        if (element && element.tagName !== 'SELECT') {
            let selectTag = $('<select></select>', {
                id: element.attributes['id'].value,
                name: element.attributes['name'].value,
                class: 'form-select ' + element.attributes['class'].value,
            });
            selectTag.insertAfter(element);
            element.remove();
        }

        if (options && options.isModal) {
            defaultOptions.dropdownParent = $(selector).closest('div.modal-body')[0];
        }

        var finalOptions = $.extend(true, {}, defaultOptions, options);
        $(selector).select2(finalOptions);

        if (value && apiGet) {
            apiGet(value).then(function (data) {
                var newOption = new Option(data.text || data.displayName || data.name, data.id, false, false);
                $(selector).append(newOption).trigger('change');
            });
        }
    }

})();
