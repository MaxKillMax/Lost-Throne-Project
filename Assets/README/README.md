Written in Russian and English

This README file is written for two purposes. 
1. Firstly, it is necessary for me if I forget something.
2. Secondly, the reviewer accepting my resume will evaluate this summary of the project and save time :)

Gameplay video (2 min) on YouTube:
https://youtu.be/EmELAUKQLtY

------------------
RUSSIAN VERSION
------------------

� ����� "Lost Throne" ��������� ��� ����� ����� ����. ���� ������� �� 2 ������� �����: "Board" � "OpenWorld".
���� ��� �������������� ��������� ���� "Game/Scripts", � ������� ��������� ���������� �������, ������ �� ���� ������ (���� ��� ������ ServiceLocator)

1. "Board".
- ����� ������ � ���� ������, ������ ��� ������ ��������� ����� ����. ��� ����� ������� �������: Board � ������ BoardBase (���� � IService � ��������� � ���������� �����, �� ����� ������������� ��� "Board").
- ������ Board ������ ���������� � ��������� ����, ����� �������� ��������� ����, ��������� � ���������, ��������������� ������. ������������� ������� ����.
- ������ BoardBase ������ ��� ������, ������������� �� ���������: ����� ������� �������� �� �����, ����������� ������, �������������� ����� �� �����.
- �����, ����� ��������, ��� ��� ������������ ��������� ������ ������� BoardEnums.
- ����� ��� �� �������� BoardPlayer. �� ������ ��������, ��������� ��������� � ���: ����� � ����� ���������, ������. � ��������, ������������ ��� ������ ����������, � ��� ������ �� ������.
- �� ������ �� ����� ������� ������ Cell, ������� ������ ���� ������ 2 ������� Line: ����� ������� ����� � �������� �����.
- ������ BattleStarter �������� ��������� ������������� ����������� ������� ��������� ����. �� �����������, ��������� ���� ������ ����������� ����� "OpenWorld", �� ��� ���� ���, �.�. �� �� ��� �����,
��� core �������� - "Board".
- ������ Scripts/Inputs ��������� ����� ������ ������. ��� ��� ����������� �� BoardInput � ������������� ������������ BoardPlayer ��� ������ �����.
- ��������, PlayerInput ����������� ����� ������� BoardPlayer, � AIInput ��������� ������ ��.
- ���� Scripts/Commands �������� ������ ��������� �������, ������� ����� ���� ����������� ������� Input-���.
- � Scripts/Cards ��������� ������� ������������� ����� - �����. ��� ������� �� TowerCard � UnitCard. 
�������� ����� ����������, � ����������� ������������� TowerInput.
�������� ������ ������������ ������ ��������� ��� ����������� �� �� �����, ����� ��������� ���� � �����.


2. "OpenWorld".
- ��������� � ������ ����������. ������ ���

3. ���������� �������
- ������ � ������� �� ������ ������ ������ - BoardBase. ������������� ���������, ������������� �� ���������� ����� ����.
- Formulas �������� ��������� ������, �������� � ����������, ������������� ��� ���� ����.
��� ���������� ������������� ������, ��������� � ��������� ������: ����� ������� �� ����������� �������, ������� �� ����� ����� � �.�.

------------------
ENGLISH VERSION
------------------

The file "Lost Throne" contains all the files of the game itself. The game is divided into 2 large blocks: "Board" and "OpenWorld".
There is also an additional separate file "Game/Scripts", which contains global scripts needed in all blocks (so far this is only ServiceLocator)

1. Board.
- The board stores the data necessary for the operation of the card part of the game. The two most important scripts are: Board and the BoardBase service (although IService is in the global file, it is needed exclusively for "Board").
- The Board script stores information about the state of the game, can start a card game, check its end, initialize data. Controls the process of the game.
- The BoardBase script stores all the logic responsible for the algorithms: find the position of the card on the board, move the camera, sort the cards on the line.
- Also, it's worth noting that all enums are inside the BoardEnums script.
- Next in importance is BoardPlayer. He stores actions related specifically to him: he knows about his cards, towers. Basically, it is used to issue information, but it does nothing.
- The Cell script is hung on the cells on the board, which stores 2 Line scripts inside itself: the line of the lower enemy and the upper enemy.
- The BattleStarter script is a temporary alternative to the normal launch of a card game. Normally, the card game should be launched through "OpenWorld", but it does not exist yet, because. he's not that important
like core gameplay - "Board".
- Inside Scripts/Inputs are inputs from different sides. They all inherit from BoardInput and control a specific BoardPlayer or other part.
- For example, PlayerInput is controlled by the BoardPlayer itself, while AIInput controls AI moves.
- The Scripts/Commands file is just a collection of commands that can be implemented by different Inputs.
- Scripts/Cards contains the main interactive part - cards. They are divided into TowerCard and UnitCard.
Tower cards are immobile, and are controlled exclusively by TowerInput.
Unit cards are used by the parties themselves to move them around the map, attacking enemy cards and towers.


2. "OpenWorld".
- Is under development. There is nothing

3. Global Services
- The first and main service at the moment is BoardBase. Controls the algorithms responsible for the board game.
- Formulas is a collection of formulas, constants and algorithms that are universal for the whole game.
It contains universal data related to data calculation: find objects at a certain position, subtract armor from damage, etc.