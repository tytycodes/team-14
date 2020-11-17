import Agent
import random as rand
from collections import defaultdict

class LeagueUtil:
	def __init__(self, board, league):
		self.player_names = []
		self.board_agents = []
		self.league_agents = []
		
		LeagueQ = ''
		with open('Assets\Scripts\Connect4\league.txt', 'r') as f:
			for i in f.readlines():
				LeagueQ = i
		LeagueQ = eval(LeagueQ)
		LeagueQ = defaultdict(lambda: 0.0, LeagueQ)

		MaxTactics = Agent.Agent(board, self.select_difficulty(), 'max')
		MaxStategy = Agent.Agent(league, 'league', 'max', LeagueQ)
		RandomTactics = Agent.Agent(board, self.select_difficulty(), 'random')
		RandomStrategy = Agent.Agent(league, 'league', 'random', LeagueQ)
		
		self.player_names.append('learning strategy and tactics')
		self.board_agents.append(MaxTactics)
		self.league_agents.append(MaxStategy)

		self.player_names.append('learning tactics only')
		self.board_agents.append(MaxTactics)
		self.league_agents.append(RandomStrategy)

		self.player_names.append('learning strategy only')
		self.board_agents.append(RandomTactics)
		self.league_agents.append(MaxStategy)

		self.player_names.append('no learning')
		self.board_agents.append(RandomTactics)
		self.league_agents.append(RandomStrategy)

	def get_names(self):
		return self.player_names
		
	def get_boards(self):
		return self.board_agents
		
	def get_leagues(self):
		return self.league_agents
		
	def select_difficulty(self):
		diffdict = {1 : r'Assets\Scripts\Connect4\easy.txt',
                2 : r'Assets\Scripts\Connect4\medium.txt',
                3 : r'Assets\Scripts\Connect4\hard.txt'}
		return diffdict[rand.randint(1,3)]
		
	def get_board_agent(self, index):
		return self.board_agents[index]
		
	def get_league_agent(self, index):
		return self.league_agents[index]