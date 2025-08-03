using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SeattleRoasterProject.Core.Enums;

namespace RoasterBeansDataAccess.Models;

public class RoasterModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public int RoasterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RoasterLocation Location { get; set; }
    public string ShopURL { get; set; } = string.Empty;
    public string ImageURL { get; set; } = string.Empty;
    public string ImageClass { get; set; } = string.Empty;
    public int FoundingYear { get; set; }
    public bool IsExcluded { get; set; } = false;
    public bool ContactedForPermission { get; set; } = false;
    public bool RecievedPermission { get; set; } = false;
    public bool HasParser { get; set; } = false;
}