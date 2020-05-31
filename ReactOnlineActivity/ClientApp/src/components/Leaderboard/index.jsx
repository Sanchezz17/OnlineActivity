import React, { Component } from 'react';
import authorizeFetch from '../../utils/authorizeFetch';
import styles from './leaderboard.module.css';
import { UserStatistics } from './LeaderboardConstants';

const DEFAULT_LIMIT = 20;

export default class Leaderboard extends Component {
    constructor(props) {
        super(props);
        this.state = {
            users: [],
            loadingUsers: true
        }
    }

    componentDidMount() {
        this.fetchUsers(UserStatistics.TOTAL_SCORE);
    }

    onChangeSelectedSort = (event) => {
        const { value } = event.target;
        this.fetchUsers(value);
    }

    fetchUsers = async (desiredStatistics, limit=DEFAULT_LIMIT) => {
        const users = await authorizeFetch(`/api/users?desiredStatistics=${desiredStatistics}&limit=${limit}`);
        this.setState({
            loadingUsers: false,
            users
        });
    };

    render() {
        const { users, loadingUsers } = this.state;

        return (
            <>
                <h1>Список лидеров</h1>
                
                {loadingUsers ? <p>Загрузка игроков...</p>:
                    <>
                        <label htmlFor="leaders">Сортировать </label>

                        <select name="leaders" id="leaders" onChange={this.onChangeSelectedSort} on>
                            <option value={ UserStatistics.TOTAL_SCORE } selected='selected'>по очкам</option>
                            <option value={ UserStatistics.NUMBER_OF_GAMES_PLAYED }>по играм</option>
                            <option value={ UserStatistics.NUMBER_OF_DRAWS }>по количеству рисований</option>
                            <option value={ UserStatistics.WINS_COUNT }>по победам</option>
                        </select>

                        <section className={styles.board}>{users.map(user =>
                            <div className={styles.user} key={user.name}>
                                <div>
                                    {user.photoUrl && <img className={styles.user__image} src={user.photoUrl} alt={user.name}/>}
                                    <span className={`${styles.user__name} `}>{user.name}</span>
                                </div>
                                <p className={styles.user__score}>{user.Statistics.TotalScore}</p>
                                <p className={styles.user__gamesCount}>{user.Statistics.NumberOfGamesPlayed}</p>
                                <p className={styles.user__drawsCount}>{user.Statistics.NumberOfDraws}</p>
                                <p className={styles.user__winsCount}>{user.Statistics.WinsCount}</p>
                            </div>
                        )}
                        </section>
                    </>
                }
            </>
        );
    }
}