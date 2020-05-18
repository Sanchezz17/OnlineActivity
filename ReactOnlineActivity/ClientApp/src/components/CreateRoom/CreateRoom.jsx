import React, {Component} from 'react';
import {Redirect, withRouter} from 'react-router';
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
                pointsToWin: 100,
                isPrivateRoom: false,
                themes: []
            }
        }
    }

    renderTheme = (theme) => {
        return (
            <li className={styles.themes__theme}>
                <p>{theme.name}</p>
            </li>
        );
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
        const response = await fetch('/api/rooms', {
            method: 'POST',
            redirect: 'manual',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify(this.state.settings)
        });
        const newRoomId = await response.json();
        this.props.history.push(`/rooms/${newRoomId}`);
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
                                        this.handleSettingsChange("name", event.target.value)}
                                    required={true}/>
                            </div>
                            <div className="form-group">
                                <label>Описание</label>
                                <input
                                    className="form-control"
                                    type="text"
                                    value={this.state.settings.description}
                                    onChange={(event) => 
                                        this.handleSettingsChange("description", event.target.value)}   
                                    required={true}/>
                            </div>
                            <div className={`form-group ${styles.settings__item}`}>
                                <label>Количество игроков</label>
                                <select
                                    value={this.state.settings.maxPlayerCount}
                                    onChange={(event) => 
                                        this.handleSettingsChange("maxPlayerCount", Number(event.target.value))}
                                >
                                    <option selected value="5">5</option>
                                    <option value="10">10</option>
                                    <option value="15">15</option>
                                    <option value="20">20</option>
                                </select>
                            </div>
                            <div className={`form-group ${styles.settings__item}`}>
                                <label>Количество очков для победы</label>
                                <select
                                    value={this.state.settings.pointsToWin}
                                    onChange={(event) => 
                                        this.handleSettingsChange("pointsToWin", Number(event.target.value))}
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
                                        this.handleSettingsChange("roundTime", Number(event.target.value))}
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
                                        this.handleSettingsChange("isPrivateRoom", event.target.checked)}
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
        )
    }
}

export default withRouter(CreateRoom)