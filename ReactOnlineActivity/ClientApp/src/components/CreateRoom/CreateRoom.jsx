import React, {Component} from 'react';
import {withRouter} from 'react-router';
import authorizeFetch from '../../utils/authorizeFetch';
import styles from './createRoom.module.css';

class CreateRoom extends Component {
    constructor(props) {
        super(props);

        this.state = {
            loading: false,
            settings: {
                name: '',
                description: '',
                roundTime: 60,
                maxPlayerCount: 5,
                minPlayerCount: 2,
                pointsToWin: 100,
                isPrivateRoom: false,
                isSelected: false,
                themes: [],
            }
        };
    }

    async componentDidMount() {
        await this.fetchThemes();
    }

    async fetchThemes() {
        const themes = await authorizeFetch('/api/themes');
        this.setState({
            loading: false,
            settings: {
                name: '',
                description: '',
                roundTime: 60,
                maxPlayerCount: 5,
                pointsToWin: 100,
                isPrivateRoom: false,
                themes: themes || []
            }
        });
    }

    renderTheme = (theme) => {
        return (

            <li onClick={() => this.onThemeClicked(theme)} className={styles.themes__theme} style={{
                backgroundImage: `url(${theme.pictureUrl})`, backgroundRepeat: 'no-repeat'
            }}>
                {theme.isSelected &&
                <div className={styles.themes__selected}>
                    <p className={styles.themes__text}>
                        {theme.name}
                    </p>
                </div>
                }
                {!theme.isSelected &&
                <div className={styles.themes__not_selected}>
                    <p className={styles.themes__text}>
                        {theme.name}
                    </p>
                </div>
                }
            </li>
        );
    }

    onThemeClicked(theme) {
        theme.isSelected = !theme.isSelected;
        this.setState({ themes: this.state.themes});
    }

    handleSettingsChange = (fieldName, newValue) => {
        this.setState({
            settings: {
                ...this.state.settings,
                [fieldName]: newValue
            }
        });
    }

    handleSubmit = async (event) => {
        event.preventDefault();
        const newRoomId = await authorizeFetch('/api/rooms', {
            method: 'POST',
            redirect: 'manual',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify(this.getRoomSettingsDto(this.state.settings))
        });
        this.props.history.push(`/rooms/${newRoomId}`);
    }

    getRoomSettingsDto(settings) {
        return {
            name: settings.name,
            description: settings.description,
            roundTime: settings.roundTime,
            maxPlayerCount: settings.maxPlayerCount,
            minPlayerCount: settings.minPlayerCount,
            pointsToWin: settings.pointsToWin,
            isPrivateRoom: settings.isPrivateRoom,
            themesIds: settings.themes.filter(t => t.isSelected).map(t => t.id)
        };
    }

    render() {
        return (
            this.state.loading
                ? <div className={styles.loading}>
                    <p>Загрузка...</p>
                </div>
                : <form
                    className={styles.container}
                >
                    <div className={styles.settings}>
                        <h2 className={styles.header}>Настройки</h2>
                        <div className="form-group">
                            <label>Название</label>
                            <input
                                className="form-control"
                                type="text"
                                value={this.state.settings.name}
                                onChange={(event) =>
                                    this.handleSettingsChange('name', event.target.value)}
                                required={true}/>
                        </div>
                        <div className="form-group">
                            <label>Описание</label>
                            <input
                                className="form-control"
                                type="text"
                                value={this.state.settings.description}
                                onChange={(event) =>
                                    this.handleSettingsChange('description', event.target.value)}
                                required={true}/>
                        </div>
                        <div className={`form-group ${styles.settings__item}`}>
                            <label>Количество игроков</label>
                            <select
                                value={this.state.settings.maxPlayerCount}
                                onChange={(event) =>
                                    this.handleSettingsChange('maxPlayerCount', Number(event.target.value))}
                            >
                                <option selected value="5">5</option>
                                <option value="10">10</option>
                                <option value="15">15</option>
                                <option value="20">20</option>
                            </select>
                        </div>
                        <div className={`form-group ${styles.settings__item}`}>
                            <label>Минимальное количество игроков</label>
                            <select
                                value={this.state.settings.minPlayerCount}
                                onChange={(event) =>
                                    this.handleSettingsChange('minPlayerCount', Number(event.target.value))}
                            >
                                <option selected value="2">2</option>
                                <option value="3">3</option>
                                <option value="5">5</option>
                                <option value="7">7</option>
                            </select>
                        </div>
                        <div className={`form-group ${styles.settings__item}`}>
                            <label>Количество очков для победы</label>
                            <select
                                value={this.state.settings.pointsToWin}
                                onChange={(event) =>
                                    this.handleSettingsChange('pointsToWin', Number(event.target.value))}
                            >
                                <option selected value="100">100</option>
                                <option value="120">120</option>
                                <option value="150">150</option>
                                <option value="200">200</option>
                            </select>
                        </div>
                        <div className={`form-group ${styles.settings__item}`}>
                            <label>Время раунда</label>
                            <select
                                value={this.state.settings.roundTime}
                                onChange={(event) =>
                                    this.handleSettingsChange('roundTime', Number(event.target.value))}
                            >
                                <option selected value="30">30 секунд</option>
                                <option value="60">60 секунд</option>
                                <option value="90">90 секунд</option>
                                <option value="120">120 секунд</option>
                            </select>
                        </div>
                        <div className={`form-group ${styles.settings__item}`}>
                            <label>Приватная</label>
                            <input
                                type="checkbox"
                                checked={this.state.settings.isPrivateRoom}
                                onChange={(event) =>
                                    this.handleSettingsChange('isPrivateRoom', event.target.checked)}
                            />
                        </div>
                    </div>
                    <div className={styles.themes}>
                        <h2 className={styles.header}>Темы</h2>
                        <ul className={styles.themes__list}>
                            {this.state.settings.themes.map(theme => this.renderTheme(theme))}
                            <li className={styles.themes__theme}>
                                <a className={styles.themes__add}>+</a>
                            </li>
                        </ul>
                    </div>
                    <div className={styles.submit}>
                        <button
                            onClick={this.handleSubmit}
                            className={`btn btn-success btn-lg ${styles.submit__button}`}
                        >
                            Создать комнату
                        </button>
                    </div>
                </form>
        );
    }
}

export default withRouter(CreateRoom);