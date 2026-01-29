{{~ if Bag.PagesFolder; pagesFolder = Bag.PagesFolder + "/"; end ~}}
(function ($) {
    var l = abp.localization.getResource('{{ ProjectInfo.Name }}');

    var _service = {{ EntityInfo.Namespace + '.' + EntityInfo.Name | abp.camel_case }};
    var _createModal = new abp.ModalManager(abp.appPath + '{{ pagesFolder }}{{ EntityInfo.RelativeDirectory }}/{{ EntityInfo.Name }}/CreateModal');
    var _editModal = new abp.ModalManager(abp.appPath + '{{ pagesFolder }}{{ EntityInfo.RelativeDirectory }}/{{ EntityInfo.Name }}/EditModal');

    var _$filterForm = null;
    var _dataTable = null;

    {{~ if!Option.SkipGetListInputDto ~}}
    var _getFilter = function () {
        var input = {};
        _$filterForm.serializeArray()
            .forEach(function (data) {
                if (data.value != '') {
                    input[abp.utils.toCamelCase(data.name.replace(/{{ EntityInfo.Name }}Filter./g, ''))] = data.value;
                }
            })
        return input;
    };
    {{~ end ~}}

    $(function () {
        var _$wrapper = $('#{{ EntityInfo.Name }}Wrapper');
        var _$table = _$wrapper.find('table');
        _$filterForm = _$wrapper.find('form');
        _dataTable = _$table.DataTable(abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,{{ if !Option.SkipGetListInputDto;"//disable default searchbox"; end}}
            autoWidth: false,
            scrollCollapse: true,
            order: [[0, "asc"]],
            ajax: abp.libs.datatables.createAjax(_service.getList{{- if !Option.SkipGetListInputDto;", _getFilter"; end-}}),
            columnDefs: [
                {
                    rowAction: {
                        items:
                            [
                                {
                                    text: l('Edit'),
    {{~ if !Option.SkipPermissions ~}}
                                    visible: abp.auth.isGranted('{{ ProjectInfo.Name }}.{{ EntityInfo.Name }}.Update'),
    {{~ end ~}}
                                    action: function (data) {
    {{~ if EntityInfo.CompositeKeyName ~}}
                                        _editModal.open({
        {{~ for prop in EntityInfo.CompositeKeys ~}}
                                            {{ prop.Name | abp.camel_case}}: data.record.{{ prop.Name | abp.camel_case}}{{if !for.last}},{{end}}
        {{~ end ~}}
                                        });
    {{~ else ~}}
                                        _editModal.open({ id: data.record.id });
    {{~ end ~}}
                                    }
                                },
                                {
                                    text: l('Delete'),
    {{~ if !Option.SkipPermissions ~}}
                                    visible: abp.auth.isGranted('{{ ProjectInfo.Name }}.{{ EntityInfo.Name }}.Delete'),
    {{~ end ~}}
                                    confirmMessage: function (data) {
                                        return l('{{ EntityInfo.Name }}DeletionConfirmationMessage', data.record.id);
                                    },
                                    action: function (data) {
    {{~ if EntityInfo.CompositeKeyName ~}}
                                        _service.delete({
        {{~ for prop in EntityInfo.CompositeKeys ~}}
                                                {{ prop.Name | abp.camel_case}}: data.record.{{ prop.Name | abp.camel_case}}{{if !for.last}},{{end}}
        {{~ end ~}}
                                            })
    {{~ else ~}}
                                        _service.delete(data.record.id)
    {{~ end ~}}
                                            .then(function () {
                                                abp.notify.info(l('SuccessfullyDeleted'));
												_dataTable.ajax.reloadEx();
                                            });
                                    }
                                }
                            ]
                    }
                },
                {{~ for prop in EntityInfo.Properties ~}}
                {{~ if prop | abp.is_ignore_property || string.starts_with prop.Type "List<"; continue; end ~}}
                {
                    title: l('{{ EntityInfo.Name + prop.Name }}'),
                    data: "{{ prop.Name | abp.camel_case }}"
                },
                {{~ end ~}}
            ]
        }));

{{~ if!Option.SkipGetListInputDto ~}}
		function debounce(func, delay) {
			let timerId;
			return function(...args) {
				clearTimeout(timerId);
				timerId = setTimeout(() => {
					func.apply(this, args);
				}, delay);
			};
		}
        _$filterForm.find(":input").on('input', debounce(function () {
            _dataTable.ajax.reloadEx();
		}, 300));
{{~ end ~}}
        

        _createModal.onResult(function () {
            _dataTable.ajax.reloadEx();
        });

        _editModal.onResult(function () {
            _dataTable.ajax.reloadEx();
        });

        $('#New{{ EntityInfo.Name }}Button').click(function (e) {
            e.preventDefault();
            _createModal.open();
        });
    });
})(jQuery);
