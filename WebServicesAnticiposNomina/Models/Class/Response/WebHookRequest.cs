 public class WebHookRequest
{
    public string id { get; set; }
    public string event_key { get; set; }
    public DateTime created_at { get; set; }
    public Content content { get; set; }
}

public class Content
{
    public string id { get; set; }
    public string batch_id { get; set; }
    public string? external_id { get; set; }
    public string type { get; set; }
    public string geo { get; set; }
    public Status status { get; set; }
    public string source_id { get; set; }
    public Source source { get; set; }
    public string destination_id { get; set; }
    public Destination destination { get; set; }
    public string currency { get; set; }
    public int amount { get; set; }
    public MetadataWebhook metadata { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}

public class Status
{
    public string state { get; set; }
    public string code { get; set; }
    public string description { get; set; }
}

public class Source
{
    public string id { get; set; }
    public string alias { get; set; }
    public string account_type { get; set; }
    public string account_number { get; set; }
    public Connectivity connectivity { get; set; }
    public string provider_id { get; set; }
    public string provider_name { get; set; }
    public long obtained_balance { get; set; }
    public DateTime obtained_balance_at { get; set; }
    public SourceMetadata metadata { get; set; }
    public string currency { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}

public class Connectivity
{
    public string status { get; set; }
    public string description { get; set; }
}

public class SourceMetadata
{
    public string cobre_tag { get; set; }
    public List<string> available_services { get; set; }
}

public class Destination
{
    public string type { get; set; }
    public string account_number { get; set; }
    public string counter_party_full_name { get; set; }
    public string counter_party_id_type { get; set; }
    public string counter_party_id_number { get; set; }
    public string beneficiary_institution { get; set; }
}

public class MetadataWebhook
{
    public string description { get; set; }
    public string? reference { get; set; }
    public string? tracking_key { get; set; }
    public string? cep_url { get; set; }
}
