export namespace {{ EntityInfo.NamespaceLastPart }} {
  export interface State {
    {{ EntityInfo.NamespaceLastPart | abp.camel_case }}: Response;
  }

  export interface Response {
    items: {{ EntityInfo.Name }}[];
    totalCount: number;
  }

  export interface {{ EntityInfo.Name }} {
    id: string;
    {{~ for prop in EntityInfo.Properties ~}}
    {{ prop.Name | abp.camel_case }}: {{ prop.Type }};
    {{~ end ~}}
  }
}
