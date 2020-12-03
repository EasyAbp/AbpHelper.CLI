{{~ if Bag.PagesFolder; pagesFolder = Bag.PagesFolder + "/"; end ~}}
$(function () {

    var l = abp.localization.getResource('{{ ProjectInfo.Name }}');

    var service = {{ EntityInfo.Namespace + '.' + EntityInfo.Name | abp.camel_case }};
    var createModal = new abp.ModalManager(abp.appPath + '{{ pagesFolder }}{{ EntityInfo.RelativeDirectory }}/{{ EntityInfo.Name }}/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + '{{ pagesFolder }}{{ EntityInfo.RelativeDirectory }}/{{ EntityInfo.Name }}/EditModal');

    var dataTable = $('#{{ EntityInfo.Name }}Table').DataTable(abp.libs.datatables.normalizeConfiguration({
        processing: true,
        serverSide: true,
        paging: true,
        searching: false,
        autoWidth: false,
        scrollCollapse: true,
        order: [[0, "asc"]],
        ajax: abp.libs.datatables.createAjax(service.getList),
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
            {{~ if prop | abp.is_ignore_property; continue; end ~}}
            {
                title: l('{{ EntityInfo.Name + prop.Name }}'),
                data: "{{ prop.Name | abp.camel_case }}"
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
