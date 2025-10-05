namespace AlwahaManagement.Models;

public class UserStat
{
    public uint TotalUsers { get; set; }
    public uint Admins { get; set; }
    public uint CanCreate { get; set; }
    public uint CanEdit { get; set; }
    public uint CanDelete { get; set; }
}