using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using xFood.Domain.Entities;
using xFood.Infrastructure.Persistence;

namespace xFood.Infrastructure.Seeding;

public static class SeedService
{
    public static async Task EnsureSeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<xFoodDbContext>();

        // Garante que as migrações estão aplicadas
        await ctx.Database.MigrateAsync();

        // ==============================================================================
        // 1. USUÁRIOS & TIPOS (Verificação Inteligente: Só add se não houver nenhum)
        // ==============================================================================
        if (!await ctx.TypeUsers.AnyAsync())
        {
            ctx.TypeUsers.AddRange(
                new TypeUser { Description = "Usuário" },
                new TypeUser { Description = "Administrador" },
                new TypeUser { Description = "Gerente" }
            );
            await ctx.SaveChangesAsync();
        }

        if (!await ctx.Users.AnyAsync())
        {
            var roles = await ctx.TypeUsers.AsNoTracking().ToDictionaryAsync(t => t.Description, t => t.Id);
            var users = new[]
            {
                new User {
    Name = "Chef Executivo",
    Email = "admin@aurum.com",
    Password = "123",
    DateBirth = new DateTime(1985,1,1,0,0,0, DateTimeKind.Utc),
    TypeUserId = roles["Administrador"],
    Active = true
},

new User {
    Name = "Maître",
    Email = "gerente@aurum.com",
    Password = "123",
    DateBirth = new DateTime(1992,1,1,0,0,0, DateTimeKind.Utc),
    TypeUserId = roles["Gerente"],
    Active = true
},

new User {
    Name = "Atendente",
    Email = "staff@aurum.com",
    Password = "123",
    DateBirth = new DateTime(2000,1,1,0,0,0, DateTimeKind.Utc),
    TypeUserId = roles["Usuário"],
    Active = true
}
            };
            ctx.Users.AddRange(users);
            await ctx.SaveChangesAsync();
        }

        // ==============================================================================
        // 2. CATEGORIAS (Verificação Inteligente: Só add se não houver nenhuma)
        // ==============================================================================
        // 🛑 REMOVEMOS A FAXINA TOTAL DAQUI 🛑

        if (!await ctx.Categories.AnyAsync())
        {
            var categorias = new[]
            {
                new Category { Name = "Boulangerie", Description = "Pães artesanais de fermentação natural e clássicos franceses." },
                new Category { Name = "Pâtisserie",  Description = "Alta confeitaria, doces finos e entremets delicados." },
                new Category { Name = "Café & Barista", Description = "Grãos especiais, métodos de extração e bebidas quentes." },
                new Category { Name = "Brunch",      Description = "Pratos quentes e opções sofisticadas para refeições leves." }
            };
            ctx.Categories.AddRange(categorias);
            await ctx.SaveChangesAsync();
        }

        // ==============================================================================
        // 3. PRODUTOS (SEUS PRODUTOS ANTIGOS REINSTAURADOS COM FOTOS)
        // ==============================================================================
        // Verificação Inteligente: Só adiciona o cardápio inicial se a tabela estiver VAZIA.
        // Se você editou qualquer coisa na web, não rodará isso e seus dados serão preservados.
        if (!await ctx.Products.AnyAsync())
        {
            // Buscamos as categorias existentes no banco para pegar os IDs corretos (Evita erro de Chave)
            var catDict = await ctx.Categories.AsNoTracking().ToDictionaryAsync(c => c.Name, c => c.Id);
            var produtos = new List<Product>();

            // --- BOULANGERIE ---
            if (catDict.TryGetValue("Boulangerie", out int cB))
            {
                produtos.Add(P("Croissant de Manteiga", "Feito com manteiga francesa AOP, 27 camadas de laminação perfeita.", 19.90m, 50, cB, "https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=800&q=80"));
                produtos.Add(P("Pain au Chocolat", "Massa folhada crocante recheada com dois bastões de chocolate belga.", 24.50m, 40, cB, "https://images.unsplash.com/photo-1530610476181-d83430b64dcd?w=800&q=80"));
                produtos.Add(P("Baguete Tradição", "Farinha francesa T65, fermentação de 24h, casca rústica e miolo alveolado.", 15.00m, 60, cB, "https://images.unsplash.com/photo-1598965402089-897ce52e8355?w=800&q=80"));
                produtos.Add(P("Caracol de Passas", "Massa brioche folhada enrolada com creme de confeiteiro e passas ao rum.", 26.00m, 30, cB, "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=800&q=80"));
                produtos.Add(P("Brioche Nanterre", "Pão de forma extremamente macio e rico em manteiga. Fatia grossa.", 32.00m, 20, cB, "https://images.unsplash.com/photo-1621236378699-8597faf6a176?w=800&q=80"));
                produtos.Add(P("Pão de Campo Levain", "Mistura de farinha branca e centeio, sabor levemente ácido e complexo.", 35.00m, 15, cB, "https://images.unsplash.com/photo-1585478259715-876a6a81ae08?w=800&q=80"));
                produtos.Add(P("Focaccia de Alecrim", "Alta hidratação, regada com azeite extra virgem e finalizada com flor de sal.", 28.00m, 15, cB, "https://images.unsplash.com/photo-1589367920969-ab8e050bbb04?w=800&q=80"));
                produtos.Add(P("Ciabatta Rústica", "Pão italiano de alta hidratação, perfeito para antepastos.", 14.00m, 30, cB, "https://images.unsplash.com/photo-1627308595229-7830a5c91f9f?w=800&q=80"));
                produtos.Add(P("Pão de Nozes e Figo", "Fermentação natural com generosa quantidade de nozes e figos secos.", 38.00m, 12, cB, "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=800&q=80"));
                produtos.Add(P("Folhado de Maçã", "Chausson aux pommes clássico com compota caseira de maçã.", 21.50m, 25, cB, "https://images.unsplash.com/photo-1601205741712-b261aff33a7d?w=800&q=80"));
                produtos.Add(P("Kouign-Amann", "Especialidade bretã caramelizada e amanteigada. Crocância extrema.", 22.00m, 15, cB, "https://images.unsplash.com/photo-1610861623947-283dc46294d1?w=800&q=80"));
                produtos.Add(P("Bagel com Gergelim", "Massa cozida e assada, textura densa característica de NY.", 12.00m, 40, cB, "https://images.unsplash.com/photo-1593717009566-38c98b394b36?w=800&q=80"));
                produtos.Add(P("Pão de Queijo Canastra", "Receita exclusiva com blend de queijos curados da Serra da Canastra.", 9.50m, 100, cB, "https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=800&q=80"));
                produtos.Add(P("Challah Trançado", "Pão judaico levemente adocicado e macio.", 34.00m, 10, cB, "https://images.unsplash.com/photo-1588637387043-e6f508057226?w=800&q=80"));
                produtos.Add(P("Croissant de Amêndoas", "Assado duas vezes com creme frangipane e lâminas de amêndoa.", 29.90m, 20, cB, "https://images.unsplash.com/photo-1550617931-e17a7b70dce2?w=800&q=80"));
            }

            // --- PÂTISSERIE ---
            if (catDict.TryGetValue("Pâtisserie", out int cP))
            {
                produtos.Add(P("Trio de Macarons", "Seleção do dia: Pistache, Framboesa e Caramelo Salgado.", 42.00m, 30, cP, "https://images.unsplash.com/photo-1569864358642-9d1684040f43?w=800&q=80"));
                produtos.Add(P("Bomba de Pistache", "Massa choux recheada com mousseline de pistache intenso.", 28.90m, 20, cP, "https://images.unsplash.com/photo-1628190779774-0f274cb8624f?w=800&q=80"));
                produtos.Add(P("Torta de Limão", "Creme de limão siciliano azedinho e merengue italiano maçaricado.", 30.00m, 20, cP, "https://images.unsplash.com/photo-1519915028121-7d3463d20b13?w=800&q=80"));
                produtos.Add(P("Bolo Ópera", "Camadas de biscuit de café, ganache de chocolate e creme manteiga.", 45.00m, 15, cP, "https://images.unsplash.com/photo-1578985545062-69928b1d9587?w=800&q=80"));
                produtos.Add(P("Mil Folhas Baunilha", "Massa folhada caramelizada e creme diplomata de baunilha Bourbon.", 39.00m, 15, cP, "https://images.unsplash.com/photo-1587825126743-4687d903908f?w=800&q=80"));
                produtos.Add(P("Paris-Brest", "Massa choux em roda recheada com creme de avelã praliné.", 38.00m, 12, cP, "https://images.unsplash.com/photo-1600093463592-8e36ae95ef56?w=800&q=80"));
                produtos.Add(P("Tarte Tatin", "Torta de maçãs caramelizadas invertida, servida morna.", 36.00m, 15, cP, "https://images.unsplash.com/photo-1616031036577-0c7cb934a500?w=800&q=80"));
                produtos.Add(P("Canelé de Bordeaux", "Bolinho de rum e baunilha com casca crocante e interior úmido.", 16.00m, 40, cP, "https://images.unsplash.com/photo-1605690328249-1662c237887e?w=800&q=80"));
                produtos.Add(P("Madeleines de Mel", "Trio de bolinhos amanteigados com mel de laranjeira.", 18.00m, 40, cP, "https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=800&q=80"));
                produtos.Add(P("Profiteroles", "Recheados com sorvete de baunilha e calda quente de chocolate.", 34.00m, 15, cP, "https://images.unsplash.com/photo-1560155016-bd4879ae8f21?w=800&q=80"));
                produtos.Add(P("Torta de Framboesa", "Base crocante, creme de confeiteiro e framboesas frescas.", 42.00m, 12, cP, "https://images.unsplash.com/photo-1565958011703-44f9829ba187?w=800&q=80"));
                produtos.Add(P("Cheesecake Frutas", "Estilo New York com coulis rústico de frutas vermelhas.", 32.00m, 20, cP, "https://images.unsplash.com/photo-1524351199678-941a58a3df50?w=800&q=80"));
                produtos.Add(P("Brownie Belga 70%", "Chocolate intenso e nozes, úmido por dentro.", 22.00m, 30, cP, "https://images.unsplash.com/photo-1606313564200-e75d5e30476c?w=800&q=80"));
                produtos.Add(P("Tartelette Morango", "Clássica tortinha de morango com brilho de geleia.", 24.00m, 20, cP, "https://images.unsplash.com/photo-1596366211115-e36b5455574a?w=800&q=80"));
                produtos.Add(P("Trufas Belgas", "Ganache rica de chocolate 70% enrolada em cacau premium (4 unidades).", 25.00m, 50, cP, "https://images.unsplash.com/photo-1548848221-0c2e497ed557?w=800&q=80"));
            }

            // --- CAFÉ & BARISTA ---
            if (catDict.TryGetValue("Café & Barista", out int cC))
            {
                produtos.Add(P("Espresso Duplo", "Extração perfeita de grão 100% arábica. Intenso.", 12.00m, 150, cC, "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?w=800&q=80"));
                produtos.Add(P("Cappuccino Italiano", "Espresso, leite vaporizado e espuma densa. Clássico.", 18.00m, 100, cC, "https://images.unsplash.com/photo-1572442388796-11668a67e53d?w=800&q=80"));
                produtos.Add(P("Latte Macchiato", "Leite cremoso em camadas com uma dose de espresso.", 16.00m, 100, cC, "https://images.unsplash.com/photo-1551817452-19e35c210512?w=800&q=80"));
                produtos.Add(P("Cold Brew", "Extração a frio por 18h com notas de caramelo.", 24.00m, 50, cC, "https://images.unsplash.com/photo-1517701604599-bb29b5dd7359?w=800&q=80"));
                produtos.Add(P("Flat White", "Espresso duplo com microespuma de leite sedosa.", 18.00m, 80, cC, "https://images.unsplash.com/photo-1577968897966-3d4325b36b61?w=800&q=80"));
                produtos.Add(P("Café Coado V60", "Método manual que ressalta as notas frutadas do grão.", 15.00m, 60, cC, "https://images.unsplash.com/photo-1544787219-7f47ccb76574?w=800&q=80"));
                produtos.Add(P("Mocha Chocolate", "Espresso, leite e ganache de chocolate belga.", 24.00m, 60, cC, "https://images.unsplash.com/photo-1578314675249-a6910f80cc4e?w=800&q=80"));
                produtos.Add(P("Chocolate Quente", "Feito com chocolate nobre derretido e creme de leite.", 26.00m, 40, cC, "https://images.unsplash.com/photo-1542990253-0d0f5be5f0ed?w=800&q=80"));
                produtos.Add(P("Matcha Latte", "Chá verde japonês em pó com leite vaporizado.", 25.00m, 30, cC, "https://images.unsplash.com/photo-1515823664-b6e165be2471?w=800&q=80"));
                produtos.Add(P("Iced Caramel", "Latte gelado com toque de caramelo artesanal.", 21.00m, 40, cC, "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=800&q=80"));
                produtos.Add(P("Suco Verde", "Couve, limão, maçã, pepino e gengibre.", 20.00m, 30, cC, "https://images.unsplash.com/photo-1610970881699-44a5587cabec?w=800&q=80"));
                produtos.Add(P("Água Panna", "Água mineral toscana sem gás 500ml.", 16.00m, 100, cC, "https://images.unsplash.com/photo-1548839140-29a749e1cf4d?w=800&q=80"));
                produtos.Add(P("Affogato", "Uma bola de sorvete de baunilha com espresso quente.", 22.00m, 30, cC, "https://images.unsplash.com/photo-1594631252845-d9b502913028?w=800&q=80"));
                produtos.Add(P("Irish Coffee", "Café, dose de whisky e creme de leite batido.", 32.00m, 20, cC, "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?w=800&q=80"));
                produtos.Add(P("Chá Earl Grey", "Infusão clássica de bergamota.", 14.00m, 50, cC, "https://images.unsplash.com/photo-1594631252845-d9b502913028?w=800&q=80"));
            }

            // --- BRUNCH ---
            if (catDict.TryGetValue("Brunch", out int cBr))
            {
                produtos.Add(P("Avocado Toast", "Pão levain tostado, creme de avocado, ovo poché e rabanete.", 46.00m, 30, cBr, "https://images.unsplash.com/photo-1525351484163-7529414395d8?w=800&q=80"));
                produtos.Add(P("Croque Monsieur", "Clássico francês gratinado com presunto royale e queijo gruyère.", 52.00m, 25, cBr, "https://images.unsplash.com/photo-1634142728258-2d3080d35968?w=800&q=80"));
                produtos.Add(P("Ovos Beneditinos", "Dois ovos pochés, lombo canadense e molho holandês no brioche.", 58.00m, 20, cBr, "https://images.unsplash.com/photo-1608039829572-78524f79c4c7?w=800&q=80"));
                produtos.Add(P("Croque Madame", "A versão do Monsieur coroada com um ovo frito perfeito.", 56.00m, 20, cBr, "https://images.unsplash.com/photo-1590412200988-a436970781fa?w=800&q=80"));
                produtos.Add(P("Omelete Fines Herbes", "Omelete francesa cremosa, leve e sofisticada.", 42.00m, 25, cBr, "https://images.unsplash.com/photo-1510693206972-df098062cb71?w=800&q=80"));
                produtos.Add(P("Quiche Lorraine", "Massa brisée, bacon e queijo. Acompanha salada.", 39.00m, 15, cBr, "https://images.unsplash.com/photo-1621251992037-ee4794ed3c77?w=800&q=80"));
                produtos.Add(P("Tostada de Salmão", "Salmão defumado, cream cheese de dill e alcaparras no pão preto.", 62.00m, 20, cBr, "https://images.unsplash.com/photo-1513442542250-854d436a73f2?w=800&q=80"));
                produtos.Add(P("Rabanada Brioche", "Pain Perdu com frutas vermelhas frescas e mel.", 38.00m, 20, cBr, "https://images.unsplash.com/photo-1542318469-e0d426573c09?w=800&q=80"));
                produtos.Add(P("Bowl de Granola", "Iogurte grego natural, granola artesanal e frutas.", 32.00m, 30, cBr, "https://images.unsplash.com/photo-1511690656952-34342d2c20f0?w=800&q=80"));
                produtos.Add(P("Salada Niçoise", "Atum conservado, ovos cozidos, vagem, batata e azeitonas.", 48.00m, 15, cBr, "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800&q=80"));
                produtos.Add(P("Waffles Belgas", "Massa leve com chantilly e morangos frescos.", 34.00m, 20, cBr, "https://images.unsplash.com/photo-1562376552-0d160a2f238d?w=800&q=80"));
                produtos.Add(P("Panquecas Americanas", "Pilha de panquecas fofinhas com xarope de bordo (maple).", 38.00m, 20, cBr, "https://images.unsplash.com/photo-1528207776546-365bb710ee93?w=800&q=80"));
                produtos.Add(P("Club Sandwich", "Frango, bacon, avocado e maionese no pão de miga.", 54.00m, 15, cBr, "https://images.unsplash.com/photo-1528735602780-2552fd46c7af?w=800&q=80"));
                produtos.Add(P("Shakshuka", "Ovos pochés cozidos em molho de tomate rústico e especiarias.", 44.00m, 15, cBr, "https://images.unsplash.com/photo-1590412200988-a436970781fa?w=800&q=80"));
                produtos.Add(P("Bowl de Açaí", "Açaí puro batido com banana e morango, servido na tigela.", 36.00m, 25, cBr, "https://images.unsplash.com/photo-1511690656952-34342d2c20f0?w=800&q=80"));
            }

            ctx.Products.AddRange(produtos);
            await ctx.SaveChangesAsync();
        }
    }

    // Auxiliar para criar o produto com a imagem
    private static Product P(string name, string desc, decimal price, int stock, int categoryId, string imgUrl)
    {
        return new Product
        {
            Name = name,
            Description = desc,
            Price = price,
            Stock = stock,
            CategoryId = categoryId,
            ImageUrl = imgUrl
        };
    }
}