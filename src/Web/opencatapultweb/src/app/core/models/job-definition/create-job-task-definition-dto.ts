export interface CreateJobTaskDefinitionDto {
    name: string;
    type: string;
    provider: string;
    configs: any;
    additionalConfigs: Map<string, string>;
    sequence: number;
}