{{~ if Bag.PagesFolder; pagesFolder = Bag.PagesFolder + "/"; end ~}}
$(function () {
{{~ if !Option.SkipGetListInputDto ~}}

    $("#{{ EntityInfo.Name }}SearchBtn").on('click', () => { dataTable.ajax.reload(); })

    document.addEventListener('keydown', (event) => {
        var div = document.getElementById('{{ EntityInfo.Name }}Filter');
        if (event.key === 'Enter' && div.contains(event.target)) {
            event.preventDefault();
            dataTable.ajax.reload();
        }
    });

    //After abp v7.2 use dynamicForm 'column-size' instead of the following settings
    //$('#{{ EntityInfo.Name }}Collapse div').addClass('col-sm-3').parent().addClass('row');

    easyHelper.setFilterToggle('#{{ EntityInfo.Name }}Filter');
{{~ end ~}}

    var l = abp.localization.getResource('{{ ProjectInfo.Name }}');

    var service = {{ EntityInfo.Namespace + '.' + EntityInfo.Name | abp.camel_case }};
    var createModal = new abp.ModalManager(abp.appPath + '{{ pagesFolder }}{{ EntityInfo.RelativeDirectory }}/{{ EntityInfo.Name }}/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + '{{ pagesFolder }}{{ EntityInfo.RelativeDirectory }}/{{ EntityInfo.Name }}/EditModal');

    var dataTable = $('#{{ EntityInfo.Name }}Table').DataTable(abp.libs.datatables.normalizeConfiguration({
        processing: true,
        serverSide: true,
        paging: true,
        searching: false,{{ if !Option.SkipGetListInputDto;"//disable default searchbox"; end}}
        autoWidth: false,
        scrollCollapse: true,
        order: [[0, "asc"]],
        ajax: abp.libs.datatables.createAjax(service.getList{{- if !Option.SkipGetListInputDto;',easyHelper.serializeForm("#' + EntityInfo.Name + 'Filter")'; end-}}),
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
                                    editModal.open({
    {{~ for prop in EntityInfo.CompositeKeys ~}}
                                        {{ prop.Name | abp.camel_case}}: data.record.{{ prop.Name | abp.camel_case}}{{if !for.last}},{{end}}
    {{~ end ~}}
                                    });
{{~ else ~}}
                                    editModal.open({ id: data.record.id });
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
                                    service.delete({
    {{~ for prop in EntityInfo.CompositeKeys ~}}
                                            {{ prop.Name | abp.camel_case}}: data.record.{{ prop.Name | abp.camel_case}}{{if !for.last}},{{end}}
    {{~ end ~}}
                                        })
{{~ else ~}}
                                    service.delete(data.record.id)
{{~ end ~}}
                                        .then(function () {
                                            abp.notify.info(l('SuccessfullyDeleted'));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                }
            },
            {{~ for prop in EntityInfo.Properties ~}}
            {{~ if prop | abp.is_ignore_property || string.starts_with prop.Type "List<"; continue; end ~}}
            { title: l('{{ EntityInfo.Name + prop.Name }}'), data: "{{ prop.Name | abp.camel_case }}"
				{{- if string.starts_with prop.Type "DateTime"; ", dataFormat: 'datetime'"; end -}}
				{{- if string.starts_with prop.Type "Guid"; ", visible: false"; end -}}
				{{- if string.starts_with prop.Type "bool"; ", dataFormat: 'boolean'"; end -}}
			},
            {{~ end ~}}
        ]
    }));

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $('#New{{ EntityInfo.Name }}Button').click(function (e) {
        e.preventDefault();
        createModal.open();
    });
});
