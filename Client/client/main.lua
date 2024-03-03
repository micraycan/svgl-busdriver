-- qb-target settings -- 

CreateThread(function()
    -- Starter Ped
    local pedModel = `s_m_m_gentransport` -- change
    RequestModel(pedModel)
    while not HasModelLoaded(pedModel) do Wait(10) end
    local ped = CreatePed(0, pedModel, 453.7, -600.52, 27.59, 259.92, false, false)
    TaskStartScenarioInPlace(ped, 'WORLD_HUMAN_CLIPBOARD', true) -- play scenario (pastebin.com/6mrYTdQv)
    FreezeEntityPosition(ped, true)
    SetEntityInvincible(ped, true)
    SetBlockingOfNonTemporaryEvents(ped, true) -- make obvlivious to everything going on
    -- Target
    exports['qb-target']:AddTargetEntity(ped, {
        options = {
            {
                type = 'client',
                event = 'svgl-busdriver:client:TargetSelected',
                icon = '',
                label = 'Bus Manager',
            }
        },
        distance = 2.0
    })
end)


-- qb-menu settings --

RegisterNetEvent('svgl-busdriver:client:OpenMenu', function(data)
	local onJob = not data

	exports['qb-menu']:openMenu({
		{
			header = "Dashound",
			icon = 'fas fa-code',
			isMenuHeader = true,
		},
		{
			header = 'Start Bus Route',
			txt = 'Requires $500 deposit, pick up and transport passengers',
			icon = 'fas fa-code-merge',
			hidden = not onJob,
			params = {
				event = 'svgl-busdriver:client:StartJob'
			}
		},
		{
			header = 'Finish Job',
			txt = 'Bus deposit will be refunded if assigned bus is nearby',
			icon = 'fas fa-code-pull-request',
			hidden = onJob,
			params = {
				event = 'svgl-busdriver:client:CompleteJob'
			}
		}
	})
end)

