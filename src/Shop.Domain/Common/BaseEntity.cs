namespace Shop.Domain.Common;

public class BaseEntity
{
    public BaseEntity()
    {
        IsActive = true;
        IsDeleted = false;
        DateCreated = DateTime.Now;
    }

    public long Id { get; set; }
    public DateTime DateCreated { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public long? CreatedById { get; set; }
    public long? ModifiedById { get; set; }
    public DateTime? DateModified { get; set; }

    public T Clone<T>() where T : BaseEntity => (T)MemberwiseClone();
}
