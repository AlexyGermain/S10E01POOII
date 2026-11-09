using Commandes.Application;
using Commandes.Infrastructure;
using Commandes.Presentation;
using Xunit;

namespace Commandes.Tests;

public sealed class ObserverAtelierTests
{
    [Fact]
    [Trait("Categorie", "Atelier")]
    public void Ajout_RecoitLeMemeInstantaneDansDeuxObservateurs()
    {
        using ModeleCommande modele = CreerModele();
        ObservateurEspion premier = new ObservateurEspion();
        ObservateurEspion second = new ObservateurEspion();

        // S10-T1 : abonner les deux, ajouter Soupe × 2, puis vérifier

        IDisposable abonnementPremier = modele.Subscribe(premier);
        IDisposable abonnementSecond = modele.Subscribe(second);

        modele.Ajouter(1, 2);

        // une notification chacun, le même objet et le total de 14,30.

        Assert.Single(premier.Etats);
        Assert.Single(second.Etats);
        Assert.Same(premier.Etats.First(), second.Etats.First());

        Assert.Equal(13.00m, premier.Etats.First().Commande.Total);
    }

    [Fact]
    [Trait("Categorie", "Atelier")]
    public void Dispose_RetireUneVueEtLaisseLAutreAbonnee()
    {
        using ModeleCommande modele = CreerModele();
        ObservateurEspion premier = new ObservateurEspion();
        ObservateurEspion second = new ObservateurEspion();
        // S10-T2 : abonner les deux, désabonner le premier deux fois,
        IDisposable abonnementPremier = modele.Subscribe(premier);
        IDisposable abonnementSecond = modele.Subscribe(second);

        abonnementPremier.Dispose();
        abonnementPremier.Dispose();

        // ajouter une ligne et vérifier que seul le second reçoit l’état.
        modele.Ajouter(1, 2);

        Assert.Empty(premier.Etats);
        Assert.Single(second.Etats);
    }

    private static ModeleCommande CreerModele()
    {
        ServiceCommandes commandes = new ServiceCommandes(new DepotCommandesMemoire(), new CatalogueProduitsMemoire());
        return new ModeleCommande(commandes, new ServiceFacturation(commandes), new JournalSilencieux());
    }

    private sealed class ObservateurEspion : IObserver<EtatCommandeDto>
    {
        public List<EtatCommandeDto> Etats { get; } = new List<EtatCommandeDto>();
        public void OnNext(EtatCommandeDto etat)
        {
            Etats.Add(etat);
        }
        public void OnCompleted()
        {
        }
        public void OnError(Exception erreur)
        {
        }
    }

    private sealed class JournalSilencieux : IJournalIncidents
    {
        public void Consigner(string operation, Exception erreur)
        {
        }
    }
}
