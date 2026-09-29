# Freecrmlance — UI/UX Guidelines V1

## 1. Objectif

Freecrmlance est un outil de gestion pour freelances. Son interface doit d'abord être **efficace**.

Une décision UI n'est utile que si elle améliore au moins un de ces points :
- comprendre plus vite ;
- agir plus vite ;
- éviter une erreur ;
- retrouver une information plus vite.

La décoration seule n'est pas un objectif.

Principe produit :

> **Un client, un espace, toute la relation.**

La fiche client est le centre de l'expérience : contacts, documents, devis et factures doivent rester faciles à retrouver depuis cet espace.

---

## 2. Principes non négociables

### Mobile-first

Tout écran est conçu d'abord pour une largeur de **375 à 430 px**, puis adapté à la tablette et au desktop.

Conséquences :
- aucune action essentielle uniquement au survol ;
- aucune information essentielle dépendante d'un tableau horizontal ;
- les actions principales restent facilement accessibles au pouce ;
- les groupes d'actions se replient proprement ;
- les formulaires sont utilisables sans zoom ;
- le desktop utilise l'espace supplémentaire sans changer la logique de l'écran.

Breakpoints Bootstrap peuvent être utilisés. Ne pas créer un système responsive parallèle sans besoin réel.

### Efficacité avant esthétique

Préférer :
- une information directement visible à un clic supplémentaire ;
- un libellé explicite à une icône ambiguë ;
- une liste compacte à une succession de grandes cartes décoratives ;
- une action proche de l'objet concerné ;
- des valeurs métier lisibles à des effets visuels.

Éviter :
- hero marketing dans l'application authentifiée ;
- grandes zones vides sans fonction ;
- animations décoratives ;
- gradients et effets gratuits ;
- multiplication des cartes imbriquées ;
- composants custom quand Bootstrap répond correctement au besoin.

### Une hiérarchie évidente

À tout moment, l'utilisateur doit comprendre :
1. où il se trouve ;
2. ce qu'il regarde ;
3. quelle est l'action principale ;
4. comment revenir au contexte précédent.

Une zone fonctionnelle ne doit avoir qu'**une action primaire dominante**.

---

## 3. Identité visuelle

Direction : **sobre, professionnelle, chaleureuse et dense juste ce qu'il faut**.

Freecrmlance doit ressembler à un outil de travail fiable, pas à une landing page de startup.

### Palette V1

Couleur principale : **bleu professionnel**.

- Primary: `#2563EB`
- Primary hover: `#1D4ED8`
- Primary soft: `#EFF6FF`
- Texte principal: `#172033`
- Texte secondaire: `#667085`
- Fond application: `#F7F8FA`
- Surface: `#FFFFFF`
- Bordure: `#E4E7EC`
- Succès: `#15803D`
- Danger: `#B42318`
- Warning: `#B54708`

Le bleu sert aux actions, liens, focus et éléments actifs. Il ne doit pas colorer de grandes surfaces sans raison.

Les couleurs sémantiques sont réservées aux états correspondants.

### Typographie

Utiliser la pile système Bootstrap / navigateur. Pas de police externe en V1.

Objectifs :
- chargement immédiat ;
- rendu natif ;
- maintenance minimale.

Hiérarchie recommandée :
- titre écran : 1.5–1.75rem ;
- titre section : 1.1–1.25rem ;
- corps : environ 1rem ;
- métadonnée : .8–.875rem.

Les montants et références importantes utilisent surtout poids, alignement et espacement — pas une taille spectaculaire.

### Formes et profondeur

- rayon courant : 8–12 px ;
- bordure légère avant ombre ;
- ombres rares et discrètes ;
- pas de glassmorphism ;
- pas de relief décoratif.

---

## 4. Navigation

### Desktop

Une navigation latérale est acceptable si elle reste compacte et stable.

Elle doit :
- identifier clairement Freecrmlance ;
- montrer la section active ;
- donner accès aux fonctions principales ;
- isoler les paramètres des opérations courantes ;
- ne pas voler inutilement de largeur au contenu.

### Mobile

Navigation compacte avec menu explicite.

L'écran doit conserver :
- le titre/contexte ;
- l'action principale ;
- l'accès à la navigation.

Le menu mobile ne doit pas être une simple version réduite illisible de la sidebar desktop.

---

## 5. Structure standard d'un écran

Ordre recommandé :

1. retour/contexte si nécessaire ;
2. titre ;
3. courte information secondaire utile ;
4. action principale ;
5. contenu métier ;
6. actions secondaires/destructives.

Ne pas afficher de sous-titre générique s'il n'apporte aucune information.

Exemple inutile :
> Gérez efficacement vos clients.

Exemple utile :
> 12 clients · 3 devis en attente.

Les indicateurs ne sont affichés que si les données sont déjà disponibles proprement. Ne pas créer de requêtes coûteuses pour décorer un écran.

---

## 6. Fiche client

La fiche client est le **hub métier principal**.

Elle doit permettre de voir et atteindre rapidement :
- identité et coordonnées ;
- contacts ;
- devis ;
- factures ;
- documents.

Sur mobile, ces blocs sont empilés selon leur fréquence d'utilisation.

Sur desktop, l'espace peut permettre une disposition plus dense, mais la même logique doit être conservée.

Éviter une page interminable composée de grandes cartes identiques. Les sections métier doivent être immédiatement différenciables.

---

## 7. Listes et tableaux

### Mobile

Préférer une liste structurée lorsque le tableau devient difficile à lire.

Chaque élément montre d'abord :
- identité/référence ;
- état ;
- information métier principale ;
- action évidente.

Les informations secondaires peuvent être réduites.

### Desktop

Un tableau est préférable lorsqu'il permet de comparer rapidement plusieurs lignes.

Règles :
- colonnes limitées aux informations utiles ;
- montants alignés à droite ;
- actions regroupées ;
- ligne entière cliquable seulement si son comportement reste évident ;
- pas de tableau dans une carte uniquement pour ajouter une bordure.

---

## 8. Formulaires

Les formulaires sont conçus pour être remplis vite.

- un champ par ligne sur mobile ;
- plusieurs colonnes sur desktop uniquement pour des champs naturellement liés ;
- labels toujours visibles ;
- aide uniquement lorsqu'elle évite une ambiguïté ;
- validation près du champ concerné ;
- valeurs et formats fr-FR ;
- bouton principal explicite : `Enregistrer`, `Créer le devis`, `Émettre la facture`, etc.

Éviter `Valider` lorsque l'action réelle peut être nommée.

Pour les formulaires longs, regrouper les champs par sections métier.

---

## 9. Actions

Hiérarchie :
- **Primary** : action principale ;
- **Secondary / outline** : action alternative ;
- **Link / quiet** : navigation ou action légère ;
- **Danger** : suppression/révocation destructive.

Une suppression ne doit jamais devenir l'action visuellement dominante.

Sur mobile :
- boutons tactiles suffisamment grands ;
- action principale pleine largeur lorsque cela facilite réellement l'usage ;
- éviter quatre petits boutons côte à côte.

---

## 10. États métier

Les états conservent le même vocabulaire et la même sémantique partout.

### Devis

- Brouillon : neutre
- Envoyé : bleu
- Accepté : vert
- Refusé : rouge discret

### Facture

- Brouillon : neutre
- Émise : vert ou bleu selon le contexte, mais choix cohérent dans toute l'application

Un badge contient toujours un texte. La couleur seule ne porte jamais l'information.

---

## 11. Empty states, erreurs et confirmations

### Empty state

Répondre à trois questions :
- qu'est-ce qui manque ?
- pourquoi cela peut être utile ?
- quelle action permet de commencer ?

Rester compact. Pas d'illustration obligatoire.

### Erreur

Dire :
- ce qui n'a pas fonctionné ;
- ce que l'utilisateur peut faire ensuite.

Éviter d'exposer des détails techniques.

### Destruction

Demander confirmation lorsque la conséquence est significative ou irréversible.

Nommer explicitement l'objet concerné lorsque possible.

---

## 12. Devis et factures

Dans l'application, ces écrans sont des **outils de gestion**, pas une imitation plein écran d'une feuille A4.

Priorités :
1. état ;
2. référence ;
3. client ;
4. montant ;
5. lignes ;
6. actions du workflow ;
7. informations légales/compliance.

Le PDF reste le document destiné à l'impression ou au partage formel.

Sur mobile, les lignes peuvent passer d'un tableau à une présentation empilée si nécessaire.

---

## 13. Accessibilité minimale

Obligatoire :
- contraste suffisant ;
- focus clavier visible ;
- labels de formulaires ;
- actions compréhensibles sans couleur ;
- zones tactiles confortables ;
- HTML sémantique ;
- modales utilisables au clavier ;
- ne pas supprimer l'outline sans remplacement visible.

Objectif tactile : environ **44 px** pour les actions principales sur mobile.

---

## 14. Langue et formats

Langue produit V1 : **fr-FR**.

Le code, le domaine et les APIs restent en anglais.

Terminologie :
- Customer → Client
- Contact → Contact
- Document → Document
- Quote → Devis
- Invoice → Facture
- Draft → Brouillon
- Sent → Envoyé
- Accepted → Accepté
- Rejected → Refusé
- Issued → Émise
- Workspace settings → Paramètres de l'entreprise

Formats :
- dates françaises ;
- nombres français ;
- euros selon la culture fr-FR ;
- éviter d'afficher UTC à l'utilisateur final sauf besoin métier explicite.

---

## 15. Règle de développement UI

Le travail se fait **écran par écran**.

Pour chaque écran :
1. partir de l'existant fonctionnel ;
2. identifier les problèmes d'usage ;
3. proposer le changement minimal qui les résout ;
4. implémenter mobile-first ;
5. vérifier mobile et desktop ;
6. faire valider visuellement ;
7. seulement ensuite passer à l'écran suivant.

Les styles globaux ne sont ajoutés que lorsqu'un motif est réellement réutilisé.

Ne pas créer un composant, une abstraction CSS ou une partial simplement parce qu'elle pourrait être utile plus tard.

---

## 16. Definition of Done UI

Un écran est terminé lorsque :
- son objectif principal est évident en quelques secondes ;
- il fonctionne à 375 px sans perte fonctionnelle ;
- il fonctionne sur desktop sans espace gaspillé ;
- l'action principale est évidente ;
- les états métier sont cohérents ;
- aucune information essentielle n'est cachée derrière un hover ;
- clavier/focus restent utilisables ;
- les textes sont en français correct ;
- aucune route, action MVC ou propriété C# n'a été traduite par erreur ;
- build et tests restent verts ;
- le rendu a été vérifié manuellement avant de passer à l'écran suivant.

---

## 17. Question de contrôle

Avant tout ajout visuel, demander :

> **Est-ce que cela aide réellement le freelance à comprendre ou agir plus vite ?**

Si la réponse est non, ne pas l'ajouter.
