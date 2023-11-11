using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Google.Maps.Models.Response
{
    internal class Data
    {
        public string? text { get; set; }
        public int value { get; set; }
    }
    internal class Element
    {
        public Data? distance { get; set; }
        public Data? duration { get; set; }
    }

    internal class Row
    {
        public List<Element>? elements { get; set; }
    }

    internal class GoogleMapsDistancesResponse
    {
        public List<string>? destination_addresses { get; set; }
        public List<string>? origin_addresses { get; set; }
        public List<Row>? rows { get; set; }
        public string? status { get; set; }
    }

    public record MapElement
    {
        public required string Address { get; init; }
        public IReadOnlyCollection<Road> Roads { get; init; } = null!;
        public record Road 
        {
            public required string Address { get; init; }
            public required int Duration { get; init; }
            public required int Distance { get; init; }
        }
    }
}
