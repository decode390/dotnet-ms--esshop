namespace Catalog.API.Products.GetProductById;

public record GetProductByIdQuery(Guid Id): IQuery<GetProductByIdResult>;
public record GetProductByIdResult(Product Product);

internal class GetProductByIdQueryHandler(
    ILogger<GetProductByIdQueryHandler> logger,
    IDocumentSession session
)
: IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
{
    public async Task<GetProductByIdResult> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("GetProductsByIdQueryHandler.Handle called with {@Query}", query);
        var product = await session.LoadAsync<Product>(query.Id, cancellationToken);
        
        if (product is null)
            throw new ProductNotFoundException();

        // var product = await session.Query<Product>().Where(o => o.Id == query.Id).FirstOrDefaultAsync(cancellationToken);
        return new GetProductByIdResult(product);
    }
}
