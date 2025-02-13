# Dont Drop the Torch

Dont Drop the Torch - is a 2D Multiplayer game made with ```Unity 2021``` and ```Netcode For GameObject```.

# Game description

## Player

Player has ```Primary Weapon``` and ```Secondary Tools```

* ```Primary Weapon``` is the same for everybody, but depending on updates applied during ```Trading Phase``` gameplay may vary.
* ```Secondary Tools``` can be baught/updated from ```Trader```, such as ```Grapling Hook```, ```Explosives``` or ```Torch```.

Players' main goal is to survive for as long as possible, balancing between fire power and durability.

## Enemy

Enemies can vary in their primary attribute(eg. Strength, Speed, Durability), depending on Players attributes development during the game.

Enemies are designed, so they wouldn't just gather in one large group that pursuits the player from behind, but rather surrond player by predicting players movement direction.

Enemies drop valuables depending on their progression level, which can later be sold during ```Trading Phase```.

## Trader

```Trader``` is being spawned every 40 seconds. Players have to find ```Trader```.

```Trading Phase``` starts only when all *alive* players have entered ```Trader``` zone.

Players automatically convert gathered goods into gold which can be spent to update ```Primary Weapon``` or ```Secondary Tools```.

Available updates are selected randomly from variety of player attributes. Updates are divided into 3 sections:

* ```Common``` - update basic attributes with improvement for up to *5%*. (eg. FireRate, FirePower)
* ```Uncommon``` - update attributes with improvement from *6% - 9%*. (eg. FirePower, ProjectileSpeed, ProjectileSpreadAngle, HealthAmount)
* ```Rare``` - updates that define gameplay strategy with improvement for up to *12%*. (eg. ProjectileAmount, MovementSpeed, DamageReduction)
* ```Legendary``` - only applicable to ```Secondary Tools```. (eg. update Torch light radius, Grapling Hook distance, Explosives radius)

## Environment

Procedurally generates obstacles like trees and rocks to put player in discomfort of not just pitch darkness but also tight surrounding. 

Additionally, makes it harder for players to gather together and keep moving as one, inevitably some of the players will get stuck, which puts the rest of the team in tough decision to wait or charge forward without lost teammate.

On the other hand trees and rocks can be used to maneuver with ```Grapling Hook```, or to make ```Enemies``` stumble into obstacles to lose them behind.

## Attributes

Core idea was to create scalable ```Attributes``` so that players would be able to change gameplay how they see fit.

Remove ```Primary Weapon``` to force players to utilize ```Environment```s' full potential to completely avoid or methodically get out ```Enemies``` one by one with ```Cold Weapon```.

Another way would be to increase fire power from the very start, but also increase amount of enemies spawned totaly and per each player separately, to create a messy bloodbath.
